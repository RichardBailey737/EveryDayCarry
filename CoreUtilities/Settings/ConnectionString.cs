using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Settings
{
    /// <summary>
    /// Event handler for the GetRepository event
    /// </summary>
    /// <returns></returns>
    public delegate string GetRepositoryHandler();

     /// <summary>
     /// DLL wide connection string for use with data functions
     /// </summary>
    public static class ConnectionString
    {
        
        /// <summary>
        /// The actual connection string
        /// </summary>
        public static string Value { get; set; }


        /// <summary>
        /// Event used to set the connection string name.  Gives us the ability to set the connection string name on each call by the service using the library 
        /// (can set the connection string to the repository in the session of the user)
        /// </summary>
        public static event GetRepositoryHandler GetRepository;

        /// <summary>
        /// The name of the connection string to use.  Connection could be stored in the apprpriate .config file
        /// </summary>
        public static string Name
        {
            get
            {
                   if (GetRepository != null)
                    {
                        return GetRepository();
                    } else
                    {
                        throw new Exception("NO REPOSITORY");
                    }
                    //if (System.Web.HttpContext.Current.Session["repository"] != null)
                    //{
                    //    return System.Web.HttpContext.Current.Session["repository"].ToString();
                    //} else
                    //{
                    //    throw new Exception("Connection string name not set");
                    //}
              
            }
    
        }

    }
}
