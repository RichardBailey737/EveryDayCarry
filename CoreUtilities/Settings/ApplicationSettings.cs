using Ensur.Core.Utilities.Database;
using Ensur.Core.Utilities.Helpers;
using Ensur.Core.Utilities.Properties;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Settings
{
    /// <summary>
    /// Caches and holds all the application system settings (stored in DCS_SYSTEM)
    /// </summary>
    public static class ApplicationSettings
    {

        private static NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }

        public delegate string MapPathHandler(string path);
        public static event MapPathHandler GetMapPathHandler;

        /// </summary>
        public static string MapApplicationPath(string path)
        {
            if (GetMapPathHandler != null)
            {
                return GetMapPathHandler(path);
            }
            else
            {
                return path;
            }
        }


        /// <summary>
        /// Loads the email configuration from the database.
        /// </summary>
        public static DCS_MAIL_SETUP MailConfig
        {
            get
            {
                if (EnsurCache.Instance.Get<DCS_MAIL_SETUP>("EMAILSETUP") == null)
                {
                    var setup = DCS_MAIL_SETUP.FirstOrDefault(new Sql());
                    EnsurCache.Instance["EMAILSETUP"] = setup;
                    return setup;
                }
                else
                {
                    return EnsurCache.Instance.Get<DCS_MAIL_SETUP>("EMAILSETUP");
                }
            }
        }

        /// <summary>
        /// Gets a list of all the repositories in the configuration.  Can be configured as add key="repository.1" value="Dev,Dev DB CI Server"  where "Dev DB CI Server" or add key = "repository.2" value="CI" 
        public static Dictionary<string, string> Repositories
        {
            get
            {

                Dictionary<string, string> rtrn = new Dictionary<string, string>();
                var keys = ConfigurationManager.AppSettings.AllKeys.Where(key => key.ToUpper().StartsWith("REPOSITORY"));
                foreach (var key in keys)
                {
                    if (ConfigurationManager.AppSettings[key].Contains(","))
                    {
                        var splt = ConfigurationManager.AppSettings[key].Split(",".ToCharArray());
                        rtrn.Add(splt[1], splt[0]);
                    }
                    else
                    {
                        var splt = key.Split(".".ToCharArray());
                        rtrn.Add(splt[0], ConfigurationManager.AppSettings[key]);
                    }

                }
                return rtrn;

            }
        }

        /// <summary>
        /// Retrieves a list of domains for the specified repository
        /// </summary>
        /// <param name="Repository">The VALUE of the repository (connection string name)</param>
        /// <returns></returns>
        public static string[] FetchDomains(string Repository)
        {
            try
            {
                EnsurDatabaseDB db = new EnsurDatabaseDB(Repository);
                return db.Fetch<DCS_DOMAIN>(new Sql().Where("ACTIVE = 1")).Select(d => d.DOMAIN_NAME).ToArray();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error fetching domain for respository {Repository}");
                throw new UserError(Resources.GENERAL_ERROR);
            }
        }

        /// <summary>
        /// Cached copy of the DCS_SYSTEM entry
        /// </summary>
        public static DCS_SYSTEM Settings
        {
            get
            {
                if (!EnsurCache.Instance.Exists("APPSETTINGS"))
                {
                    var val = SessionCache.Instance.BySQL<DCS_SYSTEM>(new Sql(), "DCS_SYSTEM");
                    EnsurCache.Instance.Put("APPSETTINGS", val, new string[] { "DCS_SYSTEM" });
                    return val;
                }
                return EnsurCache.Instance.Get<DCS_SYSTEM>("APPSETTINGS");
            }
        }

        /// <summary>
        /// Cached copy of the DCS_SYSTEM_GEN entry
        /// </summary>
        public static DCS_SYSTEM_GEN GenSettings
        {
            get
            {
                if (!EnsurCache.Instance.Exists("APPGENSETTINGS"))
                {
                    var val = SessionCache.Instance.BySQL<DCS_SYSTEM_GEN>(new Sql(), "DCS_SYSTEM_GEN");
                    EnsurCache.Instance.Put("APPGENSETTINGS", val, new string[] { "DCS_SYSTEM_GEN" });
                    return val;
                }
                return EnsurCache.Instance.Get<DCS_SYSTEM_GEN>("APPGENSETTINGS");

            }
        }

    }

    public static class SettingHelpers
    {

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/checkout" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetCheckoutPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "checkout") + "\\";
            }
            else
            {
                return system.CHECKOUT_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/supportingmaterial" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetImagePath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "supportingmaterial") + "\\";
            }
            else
            {
                return system.IMAGE_PATH;
            }
        }

        public static string GetApplicationPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return ConfigurationManager.AppSettings["PathOverride"] + "\\";
            }
            else
            {
                return ConfigurationManager.AppSettings["ApplicationPath"] + "\\";
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/original" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetOrigPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "original") + "\\";
            }
            else
            {
                return system.ORIG_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/published" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetPubPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "published") + "\\";
            }
            else
            {
                return system.PUB_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/load" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetLoaderPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "load") + "\\";
            }
            else
            {
                return system.LOADER_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/temp" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetTempPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "temp") + "\\";
            }
            else
            {
                return system.TEMP_PATH;
            }
        }


        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/export" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetExportPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "export") + "\\";
            }
            else
            {
                return system.EXPORT_PATH;
            }
        }


        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/attach" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetAttachmentPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "attach") + "\\";
            }
            else
            {
                return system.ATTACHMENT_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/attach" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetContentTypePath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "contenttemplates") + "\\";
            }
            else
            {
                return system.CONTENT_TYPE_TEMPLATE;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/report" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetReportPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "report") + "\\";
            }
            else
            {
                return system.REPORT_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/email" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetEmailPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "email") + "\\";
            }
            else
            {
                return system.EMAIL_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/renditions" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetRenditionPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "renditions") + "\\";
            }
            else
            {
                return system.RENDITION_PATH;
            }
        }

        /// <summary>
        /// This function checks the configuration file for a key named "PathOverride".  If it exists, it adds "/logfiles" onto the end of that Path, otherwise it returns the value from the DCS_SYSTEM table.
        /// </summary>
        /// <returns>System checkout path setting</returns>
        public static string GetLogPath(this DCS_SYSTEM system)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToUpper() == "PATHOVERRIDE"))
            {
                return Path.Combine(ConfigurationManager.AppSettings["PathOverride"], "logfiles") + "\\";
            }
            else
            {
                return system.LOG_PATH;
            }
        }



        /// <summary>
        /// Returns a path string to the name of the system logo
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        /// <exception cref="UserError"></exception>
        public static string GetReportLogoPath(this DCS_SYSTEM system)
        {
            string path = system.GetApplicationPath();

            if (system.RPT_LOGO_SETTING.HasValue)
            {
                if (system.RPT_LOGO_SETTING.Value.ToEnum<ReportLogoSetting>() == ReportLogoSetting.Default)
                {
                    path = Path.Combine(path, "image", "DocXellent_Logo.jpg");
                }
                else
                {
                    path = Path.Combine(path, "image", system.RPT_LOGO_FILENAME);
                }
            }
            else
            {
                path = Path.Combine(path, "image", "DocXellent_Logo.jpg");
            }

            if (!File.Exists(path)) throw new UserError(Resources.FILE_NOT_FOUND, "FILE_NOT_FOUND", "ApplicationSettings.Logo", path);

            return path;
        }
    }



}
