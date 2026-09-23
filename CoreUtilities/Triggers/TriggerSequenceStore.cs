using Ensur.Core.Utilities.Classes;
using Ensur.Core.Utilities.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ensur.Core.Utilities.Triggers
{
    /// <summary>
    /// Loads named trigger sequences (serialized lists of <see cref="DCS_TRIGGER_EVENT"/>). The AI
    /// processors resolve every model call through a sequence so the provider, model, URL and request
    /// shape stay configuration rather than code.
    /// </summary>
    public interface ITriggerSequenceStore
    {
        /// <summary>
        /// Returns the steps of the sequence named <paramref name="callName"/>.
        /// </summary>
        /// <exception cref="TriggerConfigurationException">The sequence is missing, empty or invalid.</exception>
        List<DCS_TRIGGER_EVENT> GetSequence(string callName);
    }

    /// <summary>
    /// The store used by the library. Defaults to <see cref="DatabaseTriggerSequenceStore"/>
    /// (the <c>DCS_TRIGGER_CALL</c> table); hosts can swap in <see cref="JsonFileTriggerSequenceStore"/>.
    /// </summary>
    public static class TriggerSequenceStore
    {
        /// <summary>The active store.</summary>
        public static ITriggerSequenceStore Current { get; set; } = new DatabaseTriggerSequenceStore();
    }

    /// <summary>
    /// Reads <c>DCS_TRIGGER_CALL.CALL_CODE</c> for a <c>CALL_NAME</c> through <see cref="SessionCache"/>.
    /// </summary>
    public sealed class DatabaseTriggerSequenceStore : ITriggerSequenceStore
    {
        private const string CacheKeyPrefix = "DCS_TRIGGER_CALL_";

        public List<DCS_TRIGGER_EVENT> GetSequence(string callName)
        {
            if (String.IsNullOrWhiteSpace(callName))
                throw new ArgumentException("A trigger call name is required.", nameof(callName));

            string callCode;
            try
            {
                callCode = SessionCache.Instance.BySQL<string>(
                    "SELECT CALL_CODE FROM DCS_TRIGGER_CALL WHERE CALL_NAME = @0",
                    CacheKeyPrefix + callName.ToUpperInvariant(),
                    callName);
            }
            catch (Exception ex)
            {
                throw new TriggerConfigurationException(
                    "Unable to load the '" + callName + "' trigger sequence from DCS_TRIGGER_CALL.", ex);
            }

            if (String.IsNullOrWhiteSpace(callCode))
            {
                throw new TriggerConfigurationException(
                    "DCS_TRIGGER_CALL does not contain a CALL_CODE value for CALL_NAME '" + callName + "'.");
            }

            List<DCS_TRIGGER_EVENT> steps;
            try
            {
                steps = JsonConvert.DeserializeObject<List<DCS_TRIGGER_EVENT>>(callCode);
            }
            catch (Exception ex)
            {
                throw new TriggerConfigurationException(
                    "The '" + callName + "' DCS_TRIGGER_CALL value is not valid serialized DCS_TRIGGER_EVENT JSON.", ex);
            }

            if (steps == null || steps.Count == 0)
                throw new TriggerConfigurationException("The '" + callName + "' trigger sequence contains no steps.");

            return steps;
        }
    }

    /// <summary>
    /// Reads trigger sequences from a hand-editable JSON file and reloads it whenever it changes.
    /// </summary>
    /// <remarks>
    /// <para>The file is an object whose property names are sequence names and whose values are arrays
    /// of steps. Properties starting with <c>$</c> (for example <c>"$comment"</c>) are ignored at every
    /// level. Each step uses the <see cref="DCS_TRIGGER_EVENT"/> column names with these conveniences:</para>
    /// <list type="bullet">
    /// <item><c>SEQ</c> defaults to the step's position and <c>ACTIVE</c> defaults to 1.</item>
    /// <item><c>EVENT_CALL</c> may be a JSON object instead of a serialized string. An object is an
    /// <see cref="APICall"/>, and <c>CALL_TYPE_ID</c> then defaults to <see cref="TriggerCallTypes.APICall"/>.</item>
    /// <item>An <see cref="APICall.RequestBody"/> given as a JSON object or array is converted to a token
    /// template by <see cref="APICallTemplate.FromJson"/>: write normal JSON, use <c>"{TOKEN}"</c> inside
    /// strings and <c>"@{TOKEN_JSON}"</c> to insert a token's value as raw JSON.</item>
    /// <item><c>EVENT_CRITERIA</c> may be a JSON object (a <see cref="Triggers.TriggerCriteria"/>).</item>
    /// </list>
    /// </remarks>
    public sealed class JsonFileTriggerSequenceStore : ITriggerSequenceStore
    {
        private readonly string _path;
        private readonly object _lock = new object();
        private DateTime _loadedWriteTimeUtc;
        private Dictionary<string, List<DCS_TRIGGER_EVENT>> _sequences;

        public JsonFileTriggerSequenceStore(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A trigger file path is required.", nameof(path));
            _path = Path.GetFullPath(path);
        }

        /// <summary>Full path of the trigger file.</summary>
        public string FilePath => _path;

        /// <summary>Names of every sequence in the file.</summary>
        public IReadOnlyCollection<string> SequenceNames
        {
            get { lock (_lock) { return EnsureLoaded().Keys.ToList(); } }
        }

        public List<DCS_TRIGGER_EVENT> GetSequence(string callName)
        {
            if (String.IsNullOrWhiteSpace(callName))
                throw new ArgumentException("A trigger call name is required.", nameof(callName));

            lock (_lock)
            {
                if (!EnsureLoaded().TryGetValue(callName.Trim(), out var steps))
                {
                    throw new TriggerConfigurationException(
                        "Trigger file '" + _path + "' does not define a sequence named '" + callName + "'.");
                }
                return steps;
            }
        }

        private Dictionary<string, List<DCS_TRIGGER_EVENT>> EnsureLoaded()
        {
            if (!File.Exists(_path))
                throw new TriggerConfigurationException("Trigger file not found: " + _path);

            DateTime writeTime = File.GetLastWriteTimeUtc(_path);
            if (_sequences != null && writeTime == _loadedWriteTimeUtc)
                return _sequences;

            try
            {
                _sequences = Parse(File.ReadAllText(_path));
                _loadedWriteTimeUtc = writeTime;
                return _sequences;
            }
            catch (TriggerConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TriggerConfigurationException("Trigger file '" + _path + "' is invalid: " + ex.Message, ex);
            }
        }

        /// <summary>Parses trigger-file JSON. Exposed for tests and tools.</summary>
        public static Dictionary<string, List<DCS_TRIGGER_EVENT>> Parse(string json)
        {
            var root = JObject.Parse(json);
            var result = new Dictionary<string, List<DCS_TRIGGER_EVENT>>(StringComparer.OrdinalIgnoreCase);

            foreach (JProperty sequence in root.Properties().Where(p => !p.Name.StartsWith("$")))
            {
                if (!(sequence.Value is JArray stepArray) || stepArray.Count == 0)
                    throw new TriggerConfigurationException("Sequence '" + sequence.Name + "' must be a non-empty array of steps.");

                var steps = new List<DCS_TRIGGER_EVENT>();
                int position = 0;
                foreach (JToken stepToken in stepArray)
                {
                    position++;
                    if (!(stepToken is JObject step))
                        throw new TriggerConfigurationException("Sequence '" + sequence.Name + "' step " + position + " must be an object.");

                    steps.Add(ParseStep(sequence.Name, position, step));
                }
                result[sequence.Name] = steps;
            }

            return result;
        }

        private static DCS_TRIGGER_EVENT ParseStep(string sequenceName, int position, JObject step)
        {
            JToken eventCall = step["EVENT_CALL"];
            bool isApiObject = eventCall is JObject;

            string eventCallText;
            if (eventCall == null || eventCall.Type == JTokenType.Null)
                throw new TriggerConfigurationException("Sequence '" + sequenceName + "' step " + position + " has no EVENT_CALL.");
            else if (isApiObject)
                eventCallText = ApiCallObjectToJson(StripComments((JObject)eventCall));
            else
                eventCallText = eventCall.Value<string>();

            JToken criteria = step["EVENT_CRITERIA"];

            return new DCS_TRIGGER_EVENT
            {
                EVENT_ID = step.Value<int?>("EVENT_ID") ?? position,
                EVENT_TYPE_ID = step.Value<int?>("EVENT_TYPE_ID"),
                SEQ = step.Value<int?>("SEQ") ?? position,
                EVENT_DESCRIPTION = step.Value<string>("EVENT_DESCRIPTION") ?? sequenceName + " step " + position,
                EVENT_CALL = eventCallText,
                ACTIVE = step.Value<int?>("ACTIVE") ?? 1,
                CALL_TYPE_ID = step.Value<int?>("CALL_TYPE_ID") ?? (isApiObject ? (int)TriggerCallTypes.APICall : (int?)null),
                EVENT_CRITERIA = criteria == null || criteria.Type == JTokenType.Null
                    ? null
                    : criteria.Type == JTokenType.String ? criteria.Value<string>() : criteria.ToString(Formatting.None),
                CHAIN_ID = step.Value<int?>("CHAIN_ID")
            };
        }

        private static string ApiCallObjectToJson(JObject call)
        {
            JToken body = call["RequestBody"];
            if (body != null && (body.Type == JTokenType.Object || body.Type == JTokenType.Array))
                call["RequestBody"] = APICallTemplate.FromJson(body);

            // Validate the shape now so a typo is reported when the file loads, not mid-request.
            call.ToObject<APICall>();
            return call.ToString(Formatting.None);
        }

        private static JObject StripComments(JObject obj)
        {
            var copy = (JObject)obj.DeepClone();
            foreach (JProperty property in copy.Descendants().OfType<JProperty>().Where(p => p.Name.StartsWith("$")).ToList())
                property.Remove();
            foreach (JProperty property in copy.Properties().Where(p => p.Name.StartsWith("$")).ToList())
                property.Remove();
            return copy;
        }
    }

    /// <summary>
    /// Converts a JSON request body into an <see cref="APICall.RequestBody"/> token template.
    /// </summary>
    /// <remarks>
    /// API-call templates are expanded by StringTokenFormatter, which reads <c>{NAME}</c> as a token and
    /// <c>{{</c> as a literal <c>{</c>. Closing braces outside a token are already literal (and <c>}}</c> is
    /// <i>not</i> collapsed), so only opening braces are escaped. This lets a request body be authored as
    /// ordinary JSON instead:
    /// <list type="bullet">
    /// <item><c>"{TOKEN}"</c> inside a string is replaced with the token value (the quotes stay). Use it for
    /// simple values such as model names.</item>
    /// <item>A string that is exactly <c>"@{TOKEN}"</c> is replaced by the token value <i>without</i> quotes.
    /// Use it with the <c>*_JSON</c> tokens (already-serialized JSON) and numeric tokens.</item>
    /// </list>
    /// </remarks>
    public static class APICallTemplate
    {
        private static readonly Regex RawToken = new Regex(@"^@\{[A-Za-z_][A-Za-z0-9_.]*\}$", RegexOptions.Compiled);
        private static readonly Regex TokenOrBrace = new Regex(@"\{[A-Za-z_][A-Za-z0-9_.]*\}|[{}]", RegexOptions.Compiled);

        /// <summary>Builds the template text for <paramref name="body"/>.</summary>
        public static string FromJson(JToken body)
        {
            var builder = new StringBuilder();
            Write(body, builder);
            return builder.ToString();
        }

        private static void Write(JToken token, StringBuilder builder)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    builder.Append("{{");
                    bool firstProperty = true;
                    foreach (JProperty property in ((JObject)token).Properties())
                    {
                        if (!firstProperty) builder.Append(',');
                        firstProperty = false;
                        builder.Append(EscapeAllBraces(JsonConvert.ToString(property.Name))).Append(':');
                        Write(property.Value, builder);
                    }
                    builder.Append('}');
                    break;

                case JTokenType.Array:
                    builder.Append('[');
                    bool firstItem = true;
                    foreach (JToken item in (JArray)token)
                    {
                        if (!firstItem) builder.Append(',');
                        firstItem = false;
                        Write(item, builder);
                    }
                    builder.Append(']');
                    break;

                case JTokenType.String:
                    string value = token.Value<string>();
                    if (RawToken.IsMatch(value))
                    {
                        // The trailing space keeps the token visibly separate from a following brace.
                        builder.Append(value.Substring(1)).Append(' ');
                    }
                    else
                    {
                        builder.Append(EscapeBracesKeepTokens(JsonConvert.ToString(value)));
                    }
                    break;

                default:
                    builder.Append(token.ToString(Formatting.None));
                    break;
            }
        }

        private static string EscapeAllBraces(string value) => value.Replace("{", "{{");

        private static string EscapeBracesKeepTokens(string value) =>
            TokenOrBrace.Replace(value, match => match.Value == "{" ? "{{" : match.Value);
    }

    /// <summary>A trigger sequence could not be found or parsed.</summary>
    [Serializable]
    public sealed class TriggerConfigurationException : Exception
    {
        public TriggerConfigurationException(string message) : base(message) { }

        public TriggerConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
