using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Helpers
{
    /// <summary>
    /// Adds functionality to the NLog logger
    /// </summary>
    public static class LogHelpers
    {
        /// <summary>
        /// Serializes an object into Json and logs it
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="obj">The object to serialize</param>
        /// <param name="Title">A message to display before the object</param>
        public static void InfoObj(this Logger logger, Object obj, string Title = "")
        {
            if (Title != null)  logger.Info(Title);
            try
            {
                logger.Info(JsonConvert.SerializeObject(obj));
            }
            catch 
            {
            }
        }

        /// <summary>
        /// Serializes an object into Json and logs it
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="obj">The object to serialize</param>
        /// <param name="Title">A message to display before the object</param>
        public static void WarnObj(this Logger logger, Object obj, string Title = "")
        {
            if (Title != null) logger.Warn(Title);
            try
            {
                logger.Warn(JsonConvert.SerializeObject(obj));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Serializes an object into Json and logs it
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="obj">The object to serialize</param>
        /// <param name="Title">A message to display before the object</param>
        public static void ErrorObj(this Logger logger, Object obj, string Title = "")
        {
            if (Title != null) logger.Error(Title);
            try
            {
                logger.Error(JsonConvert.SerializeObject(obj));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Serializes an object into Json and logs it
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="obj">The object to serialize</param>
        /// <param name="Title">A message to display before the object</param>
        public static void DebugObj(this Logger logger, Object obj, string Title = "")
        {
            if (Title != null) logger.Debug(Title);
            try
            {
                logger.Debug(JsonConvert.SerializeObject(obj));
            }
            catch
            {
            }
        }
    }
}
