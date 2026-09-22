using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ensur.Core.Utilities.Settings
{

    public delegate string GetSessionCacheIDHandler();
    public static class Cache
    {
        private static NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }

        public static event GetSessionCacheIDHandler GetSessionID;

        public static string SessionID
        {
            get
            {

                string sessionid = HttpContext.Current?.Items["SESSIONID"]?.ToString();

                if (!string.IsNullOrEmpty(sessionid))
                {
                    logger.Trace("Session ID retrieved: " + sessionid);
                    return sessionid;
                }
                else
                {
                    if (HttpContext.Current != null)
                    {
                        var guid = Guid.NewGuid().ToString();
                        HttpContext.Current.Items["SESSIONID"] = guid;
                        return guid;
                    }
                }

                //if (GetSessionID != null)
                //{

                //    var session = GetSessionID();
                //    logger.Trace("Session ID retrieved: " + session);
                //    return session;
                //}
                //else
                //{
                //    logger.Warn("Get Session not handled!  Returning default");
                //}

                return "1";
            }
        }

    }
}
