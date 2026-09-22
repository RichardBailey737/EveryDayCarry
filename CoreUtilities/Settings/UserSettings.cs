using Ensur.Core.Utilities.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Settings
{
    public delegate int GetUserHandler();

    public static class UserSettings
    {
     
        public static event GetUserHandler GetUser;
        public static event GetUserHandler GetImpersonationUser;

        /// <summary>
        /// The ID of the currently logged in user.  
        /// </summary>
        public static int UserID { get
            {
                if (GetUser != null)
                {
                    return GetUser();
                }
                else
                {
                    throw new Exception("User handler not implemented.");
                }
            } 
        }

        /// <summary>
        /// A boolean to indicate if impersonation is being used.
        /// </summary>
        public static bool ImpersonationUsed { get {  return UserID != ImpersonationID; } }

        /// <summary>
        /// The ID of a user being impersonated.  Only for logging/information not for security checks.
        /// </summary>
        public static int ImpersonationID
        {
            get
            {
                if (GetImpersonationUser != null)
                {
                    return GetImpersonationUser();
                }
                else
                {
                    return UserID;
                }
            }
        }
    }
}
