using Core.Extensibility.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Triggers
{

    /// <summary>
    /// DT-01141, DS-70138 Req 2: RB - Class that loads any available plugin classes from external DLLs
    /// (path configured in Ensur.Core.Utilities.Settings.Triggers.PluginPath) and executes them
    /// </summary>
    public class PluginCaller
    {
        #region "Constructor"
        /// <summary>
        /// Instantiates a new plugin caller object and loads any plugins from the Triggers setting plugin path
        /// </summary>
        public PluginCaller()
        {
            if (System.IO.Directory.GetFiles(Settings.Triggers.PluginPath, "*.dll").Length > 0)
            {
                DirectoryCatalog catalog = new DirectoryCatalog(Settings.Triggers.PluginPath, "*.dll");
                CompositionContainer container = new CompositionContainer(catalog);
                
                try
                {
                    container.SatisfyImportsOnce(this);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        logger.Error("Error loading plugins:", ex);
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                logger.Info("File missing details: " + exFileNotFound.FusionLog);
                            }
                        }
                        
                    }
                }
            }
            else
            {
                logger.Info("No plugins found");
            }
        }

        #endregion

        #region "fields"
        /// <summary>
        /// Reference to Nlog logger.  
        /// </summary>
        private NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }

        /// <summary>
        /// Collection of plugins that accept a string as input and returns a dictionary of strings
        /// </summary>
        [ImportMany(typeof(ICorePlugin<string, Dictionary<string, string>>))]
        IEnumerable<Lazy<ICorePlugin<string, Dictionary<string, string>>, IDictionary<string, object>>> StringPlugins = null;

        /// <summary>
        /// Collection of plugins that accept an object value and returns a dictionary of strings
        /// </summary>
        [ImportMany(typeof(ICorePlugin<object, Dictionary<string, string>>))]
        IEnumerable<Lazy<ICorePlugin<object, Dictionary<string, string>>, IDictionary<string, object>>> ObjectPlugins = null;

        /// <summary>
        /// Collection of plugins that accept an dictionary value and returns an object
        /// </summary>
        [ImportMany(typeof(ICorePlugin<Dictionary<string, object>, object>))]
        IEnumerable<Lazy<ICorePlugin<object, Dictionary<string, string>>, IDictionary<string, object>>> ObjectOutputPlugins = null;

        #endregion


        #region "Methods"

        /// <summary>
        /// DT-01141, DS-70138 Req 3: RB Executes all of the matching plugins for this trigger type and returns a dictionary of strings
        /// </summary>
        /// <param name="input">A string, sent to the plugin and used for input</param>
        /// <param name="PluginName">The name of plugin to fire </param>
        /// <returns>string containing comma delimited list of results (if any exist)</returns>
        public Dictionary<string, string> ExecutePlugins(string input, String PluginName)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (StringPlugins != null && StringPlugins.Count() > 0)
            {
                IEnumerable<Lazy<ICorePlugin<string, Dictionary<string, string>>, IDictionary<string, object>>> plugins = null;
                try
                {
                     plugins = StringPlugins.Where(p => p.Metadata.ContainsKey("NAME") && p.Metadata["NAME"].ToString() == PluginName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error filtering plugins by metadata");
                }

                foreach (var plugin in plugins)
                {

                    var rslt = plugin.Value.ExecutePlugin(input);
                    if (rslt != null && rslt.Count > 0)
                    {
                        rslt.ToList().ForEach(kvp => rslt[kvp.Key] = kvp.Value);
                    }


                }
            } else
            {
                logger.Error("No string output plugins loaded");
                throw new Exception("No plugins loaded");
            }
            return result;
        }

        /// <summary>
        /// DT-01141, DS-70138 Req 3: RBExecutes all of the matching plugins for this trigger type and returns a dictionary of strings
        /// </summary>
        /// <param name="input">An object of any input parameters to send to the plugin.  (It's assumed that the plugin will know what type of object this is)</param>
        /// <param name="PluginName">Type of trigger to execute</param>
        /// <returns>a comma delimited string of the results (if any exist)</returns>
        public Dictionary<string, string> ExecutePlugins(Dictionary<string, object> input, string PluginName)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (ObjectPlugins != null && ObjectPlugins.Count() > 0)
            {
                IEnumerable<Lazy<ICorePlugin<object, Dictionary<string, string>>, IDictionary<string, object>>> plugins = null;
                try
                {
                    plugins = ObjectPlugins.Where(p => p.Metadata.ContainsKey("NAME") && p.Metadata["NAME"].ToString() == PluginName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error filtering plugins by metadata");
                }

                foreach (var plugin in plugins)
                {

                    var rslt = plugin.Value.ExecutePlugin(input);
                    if (rslt != null && rslt.Count > 0)
                    {
                        rslt.ToList().ForEach(kvp => result[kvp.Key] = kvp.Value);
                    }
                }
            } else
            {
                logger.Error("No object plugins loaded");
                throw new Exception("No plugins loaded");
            }
            return result;
        }

        /// <summary>
        /// Executes a single plugin with an object output (kicks back an error if there are more than one plugin with the same name.
        /// </summary>
        /// <param name="input">A dictionary of string, object to use for input</param>
        /// <param name="PluginName">Type of trigger to execute</param>
        /// <returns>an object</returns>
        public object ExecutePlugin(Dictionary<string, object> input, string PluginName)
        {
            object result = null;
            if (ObjectOutputPlugins != null && ObjectOutputPlugins.Count() > 0)
            {
                Lazy<ICorePlugin<object, Dictionary<string, string>>, IDictionary<string, object>> plugin = null;
                try
                {
                    plugin = ObjectOutputPlugins.SingleOrDefault(p => p.Metadata.ContainsKey("NAME") && p.Metadata["NAME"].ToString() == PluginName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error filtering plugins by metadata");
                }

                    result = plugin.Value.ExecutePlugin(input);
            } else
            {
                logger.Error("No object output plugins loaded");
                throw new Exception("No plugins loaded");
            }
            return result;
        }

        #endregion
    }
}
