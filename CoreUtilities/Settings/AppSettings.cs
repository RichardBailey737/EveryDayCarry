using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Ensur.Core.Utilities.Settings
{
    /// <summary>
    /// Supplies application settings (and therefore trigger tokens such as <c>{EMBEDDINGSURL}</c>)
    /// to the library.
    /// </summary>
    public interface IAppSettingsProvider
    {
        /// <summary>Returns the value for <paramref name="key"/>, or null when it is not configured.</summary>
        string Get(string key);

        /// <summary>Returns every configured key.</summary>
        IEnumerable<string> AllKeys { get; }
    }

    /// <summary>
    /// Library-wide application settings. .NET Framework hosts get <c>app.config</c>/<c>web.config</c>
    /// AppSettings by default; modern .NET hosts (appsettings.json, IConfiguration) assign
    /// <see cref="Provider"/> at startup, usually with a <see cref="DictionaryAppSettingsProvider"/>.
    /// </summary>
    public static class AppSettings
    {
        /// <summary>The active settings source.</summary>
        public static IAppSettingsProvider Provider { get; set; } = new ConfigurationManagerAppSettingsProvider();

        /// <summary>Returns the value for <paramref name="key"/>, or null when it is not configured.</summary>
        public static string Get(string key) => Provider.Get(key);

        /// <summary>Every configured key.</summary>
        public static IEnumerable<string> AllKeys => Provider.AllKeys;

        /// <summary>
        /// Copies every setting into a trigger data dictionary so API-call templates can use them as tokens.
        /// Existing entries are overwritten.
        /// </summary>
        public static void CopyTo(IDictionary<string, object> target)
        {
            foreach (string key in AllKeys)
                target[key] = Get(key);
        }
    }

    /// <summary>Reads <see cref="ConfigurationManager.AppSettings"/>.</summary>
    public sealed class ConfigurationManagerAppSettingsProvider : IAppSettingsProvider
    {
        public string Get(string key) => ConfigurationManager.AppSettings[key];

        public IEnumerable<string> AllKeys => ConfigurationManager.AppSettings.AllKeys;
    }

    /// <summary>An in-memory settings source, typically populated from a modern host's configuration.</summary>
    public sealed class DictionaryAppSettingsProvider : IAppSettingsProvider
    {
        private readonly Dictionary<string, string> _values;

        public DictionaryAppSettingsProvider(IEnumerable<KeyValuePair<string, string>> values)
        {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in values ?? Enumerable.Empty<KeyValuePair<string, string>>())
                _values[pair.Key] = pair.Value;
        }

        public string Get(string key) => key != null && _values.TryGetValue(key, out var value) ? value : null;

        public IEnumerable<string> AllKeys => _values.Keys.ToList();
    }
}
