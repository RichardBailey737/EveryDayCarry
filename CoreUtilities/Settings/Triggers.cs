using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Settings
{
    /// <summary>
    /// Settings class for trigger settings
    /// </summary>
    public static class Triggers
    {



        public delegate string GetPluginHandler();

        private static string pluginPath;

        /// <summary>
        /// Event used to set the connection string name.  Gives us the ability to set the connection string name on each call by the service using the library 
        /// (can set the connection string to the repository in the session of the user)
        /// </summary>
        public static event GetPluginHandler GetPluginPath;

        /// <summary>
        /// The full (non-relative) path to the plugin directory
        /// </summary>
        public static string PluginPath

        {
            get
            {
                if (GetPluginPath != null)
                {
                    return GetPluginPath();
                }
                else
                {
                    throw new Exception("NO PLUGIN PATH");
                }
            }

        }

        /// <summary>
        /// Number of hours that items remain in the cache (a sliding cache) before expiring
        /// </summary>
        public static int HoursUntilExpiration => int.Parse(AppSettings.Get("CacheExpiration") ?? "3");

    }
}
