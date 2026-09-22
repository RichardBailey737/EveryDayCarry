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


namespace Ensur.Core.Utilities
{
    /// <summary>
    /// A singleton that holds a lazy-loaded list of user controls loaded by external DLL.  
    /// </summary>
    public class UserControlPlugins
    {
        #region Constructor
        public UserControlPlugins()
        {
            ReloadPlugins();
        }

        #endregion

        #region Fields


        private NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }


        [ImportMany(typeof(CoreUserControlBase),  RequiredCreationPolicy =CreationPolicy.NonShared)]
        IEnumerable<Lazy<CoreUserControlBase, IDictionary<string, object>>> UserControlPluginList = null;

        #endregion


        #region Methods

        /// <summary>
        /// Manually loads the plugin list.  
        /// </summary>
        public void ReloadPlugins()
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
                        logger.Error($"Error loading plugins : {ex.ToString()}");
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
                logger.Info($"No user control plugins found in directory {Settings.Triggers.PluginPath}");
            }
        }

        /// <summary>
        /// Searches the list of imported user controls for any controls tagged with the specified metadata
        /// </summary>
        /// <param name="MetadataKey">Metadata key to search for</param>
        /// <param name="MetadataValue">Metadata value to search for</param>
        /// <returns>List of user controls that match the criteria</returns>
        /// <remarks>EnsurUserControlBase plugin must include the ExportMetaData attribute</remarks>
        public List<CoreUserControlBase> FindUserControls(string MetadataKey, string MetadataValue)
        {
            if (UserControlPluginList != null && UserControlPluginList.Count() > 0)
            {

                try
                {
                    var plugins = UserControlPluginList.Where(p => p.Metadata.ContainsKey(MetadataKey) && p.Metadata[MetadataKey].ToString() == MetadataValue);
                    if (plugins != null && plugins.Count() > 0)
                    {
                        var plugin = plugins.Select(p => p.Value).ToList();
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error filtering plugins by metadata");
                }

            }
            return new List<CoreUserControlBase>();
        }

        /// <summary>
        /// Searches the list of imported user controls for a single control tagged with the specified metadata
        /// </summary>
        /// <param name="MetadataKey">Metadata key to search for</param>
        /// <param name="MetadataValue">Metadata value to search for</param>
        /// <returns>The first user control that match the criteria</returns>
        /// <remarks>EnsurUserControlBase plugin must include the ExportMetaData attribute</remarks>
        public CoreUserControlBase FindUserControl(string MetadataKey, string MetadataValue)
        {
            if (UserControlPluginList != null && UserControlPluginList.Count() > 0)
            {

                try
                {
                    var plugins = UserControlPluginList.Where(p => p.Metadata.ContainsKey(MetadataKey) && p.Metadata[MetadataKey].ToString() == MetadataValue);
                    if (plugins != null && plugins.Count() > 0) return plugins.FirstOrDefault().Value;

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error filtering plugins by metadata");
                }

            }
            return null;
        }
        #endregion
    }
}
