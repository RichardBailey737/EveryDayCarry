using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ensur.Core.Utilities.Properties;
using System.Security.AccessControl;
using Ensur.Core.Utilities.Settings;
using Ensur.Core.Utilities.Database;
using System.Runtime.InteropServices;
using System.Collections;

namespace Ensur.Core.Utilities
{
    public static class FileUtilities
    {

        private static NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }


        /// <summary>
        /// Checks to see if the folder path exists and creates it if not. 
        /// </summary>
        /// <param name="FolderPath">The physical path to create</param>
        /// <returns>Bool - true if the folder needed creating, otherwise false.</returns>
        public static bool CreateFolderIfNeeded(string FolderPath)
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error creating directory {FolderPath}");
                throw new Exception(Resources.FILE_CREATE_ERROR.Replace("{PATH}", FolderPath));
            }

        }

        /// <summary>
        /// Gives the specified user write permissions to the specified folder
        /// </summary>
        /// <param name="DomainAndUser">The domain and user: ex 'docxellent\rbailey'</param>
        /// <param name="FolderPath">The path to the folder.</param>
        public static void SetWritePermissionsOnFolder(string DomainAndUser, string FolderPath)
        {
            try
            {
                CreateFolderIfNeeded(FolderPath);
                var info = new DirectoryInfo(FolderPath);
                var ds = new DirectorySecurity();
                ds.AddAccessRule(new FileSystemAccessRule(DomainAndUser, FileSystemRights.Modify, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                ds.SetAccessRuleProtection(false, false);
                info.SetAccessControl(ds);

                ds.RemoveAccessRuleAll(new FileSystemAccessRule(DomainAndUser, FileSystemRights.Read, AccessControlType.Allow));
                ds.SetAccessRuleProtection(false, false);
                info.SetAccessControl(ds);

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error giving {DomainAndUser} modify permissions to directory {FolderPath}");
                //throw new Exception(Resources.FILE_CREATE_ERROR.Replace("{PATH}", FolderPath));
            }
        }

  
     
    }
}
