using Ensur.Core.Utilities.Classes;
using Ensur.Core.Utilities.Settings;
using Ensur.Core.Utilities.Triggers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static Ensur.Core.Utilities.Triggers.Triggers;

namespace Ensur.Core.Utilities.AI.Searching
{
    /// <summary>
    /// Generates vector embeddings by executing a configured trigger sequence loaded from
    /// <see cref="TriggerSequenceStore.Current"/> (the <c>DCS_TRIGGER_CALL</c> table or a JSON file).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class does not know the embedding server URL or make HTTP requests directly. It
    /// loads the trigger sequence named <c>EMBEDDINGS</c>, adds AppSettings values to the
    /// trigger token dictionary, and executes the sequence through
    /// <see cref="Triggers.TriggerSequence"/>. The API provider and model therefore remain
    /// configuration, never code.
    /// </para>
    /// <para>
    /// The supplied Ollama configuration uses the modern <c>/api/embed</c> endpoint. Its
    /// <c>input</c> property accepts either one string or an array of strings, so the same
    /// trigger supports both <see cref="GenerateEmbedding"/> and
    /// <see cref="GenerateEmbeddings"/>.
    /// </para>
    /// <para>
    /// Generating a vector is intentionally explicit. Constructing or processing a
    /// <see cref="NaturalLanguageSearchPlan"/> never invokes this class automatically.
    /// </para>
    /// </remarks>
    public sealed class EmbeddingGenerator
    {
        #region Constants

        /// <summary>
        /// Default <c>DCS_TRIGGER_CALL.CALL_NAME</c> containing the embedding trigger sequence.
        /// </summary>
        public const string DefaultTriggerCallName = "EMBEDDINGS";

        /// <summary>
        /// Name of the AppSettings key containing the complete Ollama embedding endpoint URL.
        /// </summary>
        /// <example>
        /// <c>http://192.168.86.245:11434/api/embed</c>
        /// </example>
        public const string AppSettingsEmbeddingsUrlKey = "EMBEDDINGSURL";

        /// <summary>
        /// Token inserted into the trigger's request body as JSON.
        /// </summary>
        /// <remarks>
        /// The value is already JSON-serialized. The request template must use
        /// <c>{EMBEDDING_INPUT_JSON}</c> without surrounding quotation marks. This supports
        /// both a JSON string for one input and a JSON array for a batch.
        /// </remarks>
        public const string EmbeddingInputJsonToken = "EMBEDDING_INPUT_JSON";

        /// <summary>
        /// Preferred name assigned to the API result selected from <c>$.embeddings</c>.
        /// </summary>
        public const string PreferredResultKey = "EMBEDDINGS";

        #endregion

        #region Fields

        /// <summary>
        /// Name of the configured sequence to retrieve from <c>DCS_TRIGGER_CALL</c>.
        /// </summary>
        private readonly string _triggerCallName;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a generator that uses the default <c>EMBEDDINGS</c> trigger sequence.
        /// </summary>
        public EmbeddingGenerator()
            : this(DefaultTriggerCallName)
        {
        }

        /// <summary>
        /// Creates a generator using a specific manually configured trigger sequence.
        /// </summary>
        /// <param name="triggerCallName">
        /// Value of <c>DCS_TRIGGER_CALL.CALL_NAME</c> to execute.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="triggerCallName"/> is blank.
        /// </exception>
        public EmbeddingGenerator(string triggerCallName)
        {
            if (String.IsNullOrWhiteSpace(triggerCallName))
                throw new ArgumentException("A trigger call name is required.", "triggerCallName");

            _triggerCallName = triggerCallName.Trim();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates one embedding vector for arbitrary text.
        /// </summary>
        /// <param name="text">Text or search phrase to encode.</param>
        /// <param name="userId">
        /// Optional ENSUR user identifier passed to the trigger system for logging, criteria,
        /// or later trigger steps.
        /// </param>
        /// <returns>A non-empty vector returned by the configured embedding model.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> is blank.
        /// </exception>
        /// <exception cref="EmbeddingGenerationException">
        /// Thrown when configuration loading, trigger execution, or response parsing fails.
        /// </exception>
        public float[] GenerateEmbedding(string text, int? userId = null)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text is required to generate an embedding.", "text");

            List<float[]> vectors = GenerateEmbeddings(
                new List<string> { text },
                userId);

            if (vectors.Count != 1)
            {
                throw new EmbeddingGenerationException(
                    "The embedding service returned " + vectors.Count +
                    " vectors for one input. Exactly one vector was expected.");
            }

            return vectors[0];
        }

        /// <summary>
        /// Generates embedding vectors for a batch of text values in one trigger/API call.
        /// </summary>
        /// <param name="texts">
        /// Text values to encode. The returned vectors preserve this input order.
        /// </param>
        /// <param name="userId">
        /// Optional ENSUR user identifier passed to the trigger system.
        /// </param>
        /// <returns>
        /// One non-empty embedding vector for every supplied input, in matching order.
        /// </returns>
        /// <remarks>
        /// Batch generation is useful while indexing document chunks. Query and document
        /// vectors must be generated with the same embedding model and compatible dimensions.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the collection is null, empty, or contains a blank value.
        /// </exception>
        /// <exception cref="EmbeddingGenerationException">
        /// Thrown when configuration loading, trigger execution, or response parsing fails.
        /// </exception>
        public List<float[]> GenerateEmbeddings(
            IEnumerable<string> texts,
            int? userId = null)
        {
            if (texts == null)
                throw new ArgumentNullException("texts");

            List<string> input = texts.ToList();
            if (input.Count == 0)
                throw new ArgumentException("At least one text value is required.", "texts");

            if (input.Any(String.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "Embedding inputs cannot contain null, empty, or whitespace values.",
                    "texts");
            }

            EnsureEmbeddingUrlConfigured();
            List<DCS_TRIGGER_EVENT> triggerSteps = LoadTriggerSteps();
            Dictionary<string, object> triggerData = BuildTriggerData(input, userId);

            TriggerResult triggerResult;
            try
            {
                triggerResult = Instance.TriggerSequence(triggerSteps, triggerData);
            }
            catch (Exception ex)
            {
                throw new EmbeddingGenerationException(
                    "The '" + _triggerCallName + "' trigger sequence threw an exception.",
                    ex);
            }

            EnsureTriggerSucceeded(triggerResult);

            List<float[]> vectors = ParseEmbeddingVectors(triggerResult.ResultData);
            if (vectors.Count != input.Count)
            {
                throw new EmbeddingGenerationException(
                    "The embedding service returned " + vectors.Count + " vectors for " +
                    input.Count + " inputs. The counts must match.");
            }

            return vectors;
        }

        #endregion

        #region Trigger Configuration

        /// <summary>
        /// Confirms that the URL token required by the database API-call template exists.
        /// </summary>
        private static void EnsureEmbeddingUrlConfigured()
        {
            string url = AppSettings.Get(AppSettingsEmbeddingsUrlKey);
            if (String.IsNullOrWhiteSpace(url))
            {
                throw new EmbeddingGenerationException(
                    "AppSettings does not contain a non-empty '" +
                    AppSettingsEmbeddingsUrlKey + "' value. Configure the complete Ollama " +
                    "endpoint, for example http://server:11434/api/embed.");
            }
        }

        /// <summary>
        /// Loads the configured trigger sequence from <see cref="TriggerSequenceStore.Current"/>.
        /// </summary>
        private List<DCS_TRIGGER_EVENT> LoadTriggerSteps()
        {
            try
            {
                return TriggerSequenceStore.Current.GetSequence(_triggerCallName);
            }
            catch (Exception ex)
            {
                throw new EmbeddingGenerationException(
                    "Unable to load the '" + _triggerCallName + "' trigger sequence.",
                    ex);
            }
        }

        /// <summary>
        /// Creates the values consumed by the API-call template and trigger subsystem.
        /// </summary>
        private static Dictionary<string, object> BuildTriggerData(
            IList<string> input,
            int? userId)
        {
            // Send a JSON string for one item and a JSON array for a batch. Ollama's modern
            // /api/embed endpoint accepts either shape in its input property.
            object requestInput = input.Count == 1
                ? (object)input[0]
                : input.ToArray();

            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "EMBEDDING_INPUT", input.Count == 1 ? input[0] : String.Join("\n", input) },
                { EmbeddingInputJsonToken, JsonConvert.SerializeObject(requestInput) }
            };

            if (userId.HasValue)
                data["USER_ID"] = userId.Value;

            // AppSettings become trigger tokens such as {EMBEDDINGSURL}. Assignment rather
            // than Add avoids a duplicate-key exception if a caller token is added later.
            AppSettings.CopyTo(data);

            return data;
        }

        /// <summary>
        /// Converts a failed or missing trigger result into a descriptive exception.
        /// </summary>
        private void EnsureTriggerSucceeded(TriggerResult result)
        {
            if (result == null)
            {
                throw new EmbeddingGenerationException(
                    "The '" + _triggerCallName + "' trigger returned no TriggerResult.");
            }

            if (!result.Success)
            {
                throw new EmbeddingGenerationException(
                    "The '" + _triggerCallName + "' trigger failed: " +
                    (result.ResultMessage ?? "Unknown trigger error."),
                    result.ResultError);
            }
        }

        #endregion

        #region Response Parsing

        /// <summary>
        /// Extracts vectors from either the configured <c>EMBEDDINGS</c> result or the complete
        /// Ollama response retained by <see cref="APICall"/> as <c>RESULT</c>.
        /// </summary>
        private static List<float[]> ParseEmbeddingVectors(
            Dictionary<string, string> resultData)
        {
            if (resultData == null || resultData.Count == 0)
            {
                throw new EmbeddingGenerationException(
                    "The embedding trigger completed but returned no result data.");
            }

            string json = GetValueIgnoreCase(resultData, PreferredResultKey);
            if (!String.IsNullOrWhiteSpace(json))
                return ParseVectorArray(json);

            // RESULT is always the complete HTTP response produced by APICall.ExecuteCall.
            string completeResponse = GetValueIgnoreCase(resultData, "RESULT");
            if (String.IsNullOrWhiteSpace(completeResponse))
            {
                throw new EmbeddingGenerationException(
                    "The embedding trigger returned neither EMBEDDINGS nor RESULT. Configure " +
                    "JsonResultQueries to map EMBEDDINGS to $.embeddings.");
            }

            try
            {
                JObject root = JObject.Parse(completeResponse);
                JToken embeddings = GetTokenIgnoreCase(root, "embeddings");

                // Compatibility fallback for Ollama's older /api/embeddings response shape.
                if (embeddings == null)
                {
                    JToken legacyEmbedding = GetTokenIgnoreCase(root, "embedding");
                    if (legacyEmbedding != null)
                        embeddings = new JArray(legacyEmbedding);
                }

                if (embeddings == null)
                    throw new JsonException("The response contains no embeddings property.");

                return ParseVectorArray(embeddings.ToString(Formatting.None));
            }
            catch (EmbeddingGenerationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EmbeddingGenerationException(
                    "The embedding service returned invalid response JSON.",
                    ex);
            }
        }

        /// <summary>
        /// Parses a JSON array of numeric arrays and validates every vector component.
        /// </summary>
        private static List<float[]> ParseVectorArray(string json)
        {
            try
            {
                JToken token = JToken.Parse(json);

                // A result can be encoded as a JSON string by an intermediate provider.
                while (token.Type == JTokenType.String)
                    token = JToken.Parse(token.Value<string>());

                JArray vectorsArray = token as JArray;
                if (vectorsArray == null)
                    throw new JsonException("Embeddings must be a JSON array.");

                // Permit one bare vector as a compatibility convenience. The configured modern
                // Ollama endpoint normally returns an array containing one or more vectors.
                if (vectorsArray.Count > 0 &&
                    vectorsArray[0].Type != JTokenType.Array)
                {
                    // Deep-clone the values into a nested array. Passing vectorsArray directly
                    // to JArray's content constructor would enumerate and flatten its values.
                    JArray bareVector = new JArray(
                        vectorsArray.Select(component => component.DeepClone()));
                    vectorsArray = new JArray(bareVector);
                }

                var vectors = new List<float[]>();
                foreach (JToken vectorToken in vectorsArray)
                {
                    JArray numericArray = vectorToken as JArray;
                    if (numericArray == null || numericArray.Count == 0)
                        throw new JsonException("Every embedding vector must be a non-empty array.");

                    var vector = new float[numericArray.Count];
                    for (int i = 0; i < numericArray.Count; i++)
                    {
                        float value = numericArray[i].Value<float>();
                        if (Single.IsNaN(value) || Single.IsInfinity(value))
                            throw new JsonException("Embedding vectors cannot contain NaN or infinity.");

                        vector[i] = value;
                    }

                    vectors.Add(vector);
                }

                if (vectors.Count == 0)
                    throw new JsonException("The embedding array is empty.");

                int dimensions = vectors[0].Length;
                if (vectors.Any(vector => vector.Length != dimensions))
                    throw new JsonException("All returned embedding vectors must have equal dimensions.");

                return vectors;
            }
            catch (EmbeddingGenerationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EmbeddingGenerationException(
                    "Unable to parse the embedding vector response: " + json,
                    ex);
            }
        }

        /// <summary>
        /// Retrieves a result value without requiring exact key casing.
        /// </summary>
        private static string GetValueIgnoreCase(
            Dictionary<string, string> values,
            string key)
        {
            KeyValuePair<string, string> match = values.FirstOrDefault(
                pair => String.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase));

            return match.Key == null ? null : match.Value;
        }

        /// <summary>
        /// Retrieves a JSON property without requiring exact property-name casing.
        /// </summary>
        private static JToken GetTokenIgnoreCase(JObject value, string propertyName)
        {
            JProperty property = value.Properties().FirstOrDefault(
                item => String.Equals(
                    item.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase));

            return property == null ? null : property.Value;
        }

        #endregion
    }

    /// <summary>
    /// Provides explicit, manually invoked embedding helpers for a natural-language search plan.
    /// </summary>
    public static class NaturalLanguageSearchPlanEmbeddingExtensions
    {
        /// <summary>
        /// Generates an embedding for <see cref="NaturalLanguageSearchPlan.ContentSearch"/>.
        /// </summary>
        /// <param name="plan">Plan containing the retrieval phrase.</param>
        /// <param name="generator">
        /// Optional generator instance. Pass null to use an <c>EMBEDDINGS</c> trigger-backed
        /// <see cref="EmbeddingGenerator"/>.
        /// </param>
        /// <param name="userId">Optional ENSUR user identifier supplied to the trigger.</param>
        /// <returns>
        /// The generated vector, or null when the plan has no content-search phrase.
        /// </returns>
        /// <remarks>
        /// This method is a manual shortcut only. The planner does not invoke it. Returning
        /// null for a metadata-only plan lets a caller conditionally perform vector retrieval
        /// without making an unnecessary API request.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="plan"/> is null.
        /// </exception>
        public static float[] GenerateContentSearchEmbedding(
            this NaturalLanguageSearchPlan plan,
            EmbeddingGenerator generator = null,
            int? userId = null)
        {
            if (plan == null)
                throw new ArgumentNullException("plan");

            if (String.IsNullOrWhiteSpace(plan.ContentSearch))
                return null;

            EmbeddingGenerator activeGenerator = generator ?? new EmbeddingGenerator();
            return activeGenerator.GenerateEmbedding(plan.ContentSearch, userId);
        }
    }

    /// <summary>
    /// Represents a failure while loading an embedding trigger, calling the provider, or
    /// validating returned vectors.
    /// </summary>
    [Serializable]
    public sealed class EmbeddingGenerationException : Exception
    {
        /// <summary>Initializes an exception without a message.</summary>
        public EmbeddingGenerationException()
        {
        }

        /// <summary>Initializes an exception with a descriptive message.</summary>
        public EmbeddingGenerationException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes an exception with a message and original cause.</summary>
        public EmbeddingGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Rehydrates a serialized exception.</summary>
        private EmbeddingGenerationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
