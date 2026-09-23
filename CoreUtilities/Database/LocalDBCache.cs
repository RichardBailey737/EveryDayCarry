using Ensur.Core.Utilities.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;
using NLog.Internal;
using NLog;
using System.Dynamic;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using DocumentFormat.OpenXml.Spreadsheet;
using PetaPoco;

namespace Ensur.Core.Utilities.Database
{

    /// <summary>
    /// This is a utility class that helps improve speed and reduce trips to the database by sharing a database connection between multiple functions and providing a user-specific temporary cache.  The cache for this user is created on first use and destroyed at the end of a call (a particular API or web call)
    /// </summary>
    public sealed class SessionCache
    {
        private SessionCache()
        {

        }

        private Dictionary<string, LocalDBCache> Sessions = new Dictionary<string, LocalDBCache>();
        private readonly object LockObj = new object();

        private LocalDBCache GetSession()
        {
            CleanOld();
            var sessionid = Settings.Cache.SessionID;
            if (sessionid == null)
            {
                logger.Warn("Session id is null");
                sessionid = "1";
            }
            LocalDBCache rtrn = null;
            lock (LockObj)
            {
                if (!Sessions.TryGetValue(sessionid, out var cache))
                {
                    cache = new LocalDBCache();
                    Sessions[sessionid] = cache;
                }
                rtrn = cache;


            }

            return rtrn;
        }

        private void CleanOld()
        {
            lock (LockObj)
            {
                try
                {
                    var ExpiringSessions = Sessions.Where(s => (s.Value.CreatedDateTime < DateTime.Now.AddMinutes(-3)))?.Select(s=>s.Key).Distinct().ToList();
                    if (ExpiringSessions != null && ExpiringSessions.Count > 0)
                    {
                        ExpiringSessions.ForEach(sess =>
                        {
                            Sessions[sess]?.db?.Dispose();
                            Sessions.Remove(sess);
                        });

                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error cleaning the old sessions. Session  {string.Join(",", Sessions.Select(s => s.Key + " - " + s.Value.CreatedDateTime).ToArray())}");
                    if (Sessions.Any(s=>s.Value.CreatedDateTime < DateTime.Now.AddMinutes(-10)))
                    {
                        //Sessions are not expiring, emergency session wipe.  
                        logger.Error("Found sessions that exists over 10 minutes old.  Resetting the dictionary.");
                        Sessions = new Dictionary<string, LocalDBCache>();
                    }
                }

            }
        }


        /// <summary>
        /// A reference to a shared database connection.  Can be used to create transactions that span multiple functions.
        /// </summary>
        public PetaPoco.Database SharedDBConnection => GetSession().db;

        /// <summary>
        /// Uses the shared db connection to begin a transaction (same as SharedDBConnection.BeginTransaction())
        /// </summary>
        public void BeginTran() => GetSession().BeginTransaction();


        /// <summary>
        /// Commits a transaction on the shared DB connection (same as SharedDBConnection.CompleteTransaction())
        /// </summary>
        public void CommitTran() => GetSession().CommitTransaction();


        /// <summary>
        /// Rolls back a transaction on the shared db connection (same as SharedDBConnection.AbortTransaction())
        /// </summary>
        public void RollbackTran() => GetSession().RollbackTransaction();

        /// <summary>
        /// Adds an warning that doesn't terminate the code execution.  
        /// </summary>
        /// <param name="WarningMessage"></param>
        public void AddWarning(string WarningMessage)
        {
            GetSession().SessionWarnings.Add(WarningMessage);
        }

        /// <summary>
        /// Returns a string array of all the warning messages
        /// </summary>
        /// <returns></returns>
        public string[] GetWarnings()
        {
            return GetSession().SessionWarnings.ToArray();
        }

        /// <summary>
        /// Checks to see if the key exists in the cache and populates it with the population function if not then returns the object
        /// </summary>
        /// <typeparam name="T">The data type to return</typeparam>
        /// <param name="Key">The key to store in the cache</param>
        /// <param name="populationFunc">The function to call to populate the data.</param>
        /// <returns></returns>
        public T ByObj<T>(string Key, Func<T> populationFunc)
        {
            if (!SessionCache.Instance.Exists<T>(Key)) SessionCache.Instance.Set(Key, populationFunc());
            return SessionCache.Instance.Get<T>(Key);
        }

        /// <summary>
        /// Checks the local cache to see if an item exists by that name
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public bool Exists<T>(string Key) => GetSession().Exists<T>(Key);

        /// <summary>
        /// Retrieves a strongly-typed cache item 
        /// </summary>
        /// <typeparam name="T">The type of item to return</typeparam>
        /// <param name="Key">They key in the cache to check</param>
        /// <returns></returns>
        public T Get<T>(string Key) => GetSession().Get<T>(Key);

        /// <summary>
        /// Stores an object in the local session cache
        /// </summary>
        /// <param name="Key">The key to store the object under</param>
        /// <param name="Value">The value to store</param>
        public void Set(string Key, object Value) => GetSession().Set(Key, Value);

        /// <summary>
        /// Loads a batch of petapoco objects into the cache using a SQL statement. Individual values can then be retrieved via Get(ID) functions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        public List<T> Load<T>(Sql sql) => GetSession().Load<T>(sql);

        /// <summary>
        /// Uses an EnsureDatabase to retrieve a single record and return one of the properties
        /// </summary>
        /// <typeparam name="T">The data type of the database class (DCS_USER)</typeparam>
        /// <typeparam name="TReturn">The type of data to return</typeparam>
        /// <param name="ID">The primary key value of the single record (TABLE MUST HAVE AN INT PRIMARY KEY)</param>
        /// <param name="PropName">The name of the property to return a value from</param>
        /// <returns></returns>
        public TReturn ByID<T, TReturn>(int? ID, string PropName) => GetSession().GetValueByID<T, TReturn>(ID, PropName);

        /// <summary>
        /// Retrives a single database record by the primary key value
        /// </summary>
        /// <typeparam name="T">The type of database object to return (must be a petapoco object)</typeparam>
        /// <param name="ID">The primary key to query (TABLE MUST HAVE AN INT PRIMARY KEY)</param>
        /// <returns></returns>
        public T ByID<T>(int? ID) => GetSession().GetCachedObjectByID<T>(ID);

        /// <summary>
        /// Removes the object cached by ID of type T
        /// </summary>
        /// <typeparam name="T">The type of object to remove</typeparam>
        /// <param name="ID">The ID of the object to remove</param>
        public void ClearID<T>(string ID) => GetSession().DeleteEntry<T>(ID);

        /// <summary>
        /// Removes the object cached by ID of type T
        /// </summary>
        /// <typeparam name="T">The type of object to remove</typeparam>
        /// <param name="ID">The ID of the object to remove</param>
        public void ClearID<T>(int ID) => GetSession().DeleteEntry<T>(ID.ToString());

        /// <summary>
        /// Queries the database for a single record of type T using the provided SQL and stores it under the cache named KeyName
        /// </summary>
        /// <typeparam name="T">The type of object to query and return</typeparam>
        /// <param name="sql">The Petapoco sql query</param>
        /// <param name="KeyName">The name of the cache to store it under</param>
        /// <returns></returns>
        public T BySQL<T>(Sql sql, string KeyName) => GetSession().GetSingleBySQL<T>(sql, KeyName);

        /// <summary>
        /// Queries the database for a single record of type T using the provided SQL and stores it under the cache named KeyName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="KeyName"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public T BySQL<T>(string sql, string KeyName, params object[] Parameters) => GetSession().GetSingleBySQL<T>(new Sql(sql, Parameters), KeyName);

        /// <summary>
        /// Returns a list of records of type T using the petapoco SQL query and storing it in the cache (or returns the cached list if already queried)
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="sql">The Petapoco SQL query to execute</param>
        /// <param name="KeyName">The cache item to check/store the data as</param>
        /// <returns></returns>
        public List<T> BySQLList<T>(Sql sql, string KeyName) => GetSession().GetListBySQL<T>(sql, KeyName);


        /// <summary>
        /// Returns a list of records of type T using the petapoco SQL query and storing it in the cache (or returns the cached list if already queried)
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="sql">The Petapoco SQL query to execute</param>
        /// <param name="KeyName">The cache item to check/store the data as</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public List<T> BySQLList<T>(string sql, string KeyName, params object[] Parameters) => GetSession().GetListBySQL<T>(new Sql(sql, Parameters), KeyName);

        /// <summary>
        /// Returns a list of records of type T using a stored procedure and storing it in the cache (or returns the cached list if already queried)
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="SprocName">The name of the stored procedure to execute</param>
        /// <param name="KeyName">The cache item to check/store the data as</param>
        /// <param name="Parameters">An array of objects that match the order of the parameters in the stored procedure.  You must provide values to all the parameters</param>
        /// <returns></returns>
        public List<T> BySprocList<T>(string SprocName, string KeyName, params object[] Parameters) => GetSession().GetListBySproc<T>(SprocName, KeyName, Parameters);

        /// <summary>
        /// Returns a list of records of type T using a stored procedure and storing it in the cache (or returns the cached list if already queried)
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="SprocName">The name of the stored procedure to execute</param>
        /// <param name="KeyName">The cache item to check/store the data as</param>
        /// <param name="Parameters">A dictionary with keys that match the names of the parameters in the stored procedure.  </param>
        /// <returns></returns>
        public List<T> BySprocListDict<T>(string SprocName, string KeyName, Dictionary<string, object> Parameters) => GetSession().GetListBySprocDict<T>(SprocName, KeyName, Parameters);


        /// <summary>
        /// Returns a single value by executing the stored procedure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SprocName"></param>
        /// <param name="KeyName"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public T BySproc<T>(string SprocName, string KeyName, params object[] Parameters) => GetSession().GetValueBySproc<T>(SprocName, KeyName, Parameters);



        private NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }

        /// <summary>
        /// Terminates the current session and wipes out the cache values
        /// </summary>
        public void EndSession()
        {
            try
            {
                SharedDBConnection.CompleteTransaction();
            }
            catch
            {

            }
            SharedDBConnection.Dispose();
            var sessionid = Settings.Cache.SessionID;
            if (Sessions.ContainsKey(sessionid)) Sessions.Remove(sessionid);
            //Sessions = null;
        }

        /// <summary>
        /// A singleton containing the local session cache.
        /// </summary>
        public static SessionCache Instance { get { return SessionCacheNested.instance; } }

        private class SessionCacheNested
        {

            static SessionCacheNested()
            {

            }

            internal static readonly SessionCache instance = new SessionCache();
        }

    }

    /// <summary>
    /// This is a simple utility class that can help speed up repeated calls to the database.  Best used in a place with a lot of database back and forth and a large number of options/if statements. 
    /// </summary>
    public class LocalDBCache
    {
        public LocalDBCache()
        {
            
            db = new PocoDB();
            CreatedDateTime = DateTime.Now;
            SessionWarnings = new List<string>();
        }
        private Dictionary<string, object> _cache = new Dictionary<string, object>();
        public PocoDB db = null;

        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public List<string> SessionWarnings { get; set; }


        int Trancount = 0;

        public DateTime CreatedDateTime { get; private set; }

        /// <summary>
        /// Starts a transaction.  If there are multiple nested transactions, it increases the count instead of committing
        /// </summary>
        public void BeginTransaction()
        {
            if (Trancount == 0) db.BeginTransaction();
            Trancount++;
        }

        /// <summary>
        /// Aborts all transactions and resets the count
        /// </summary>
        public void RollbackTransaction()
        {
            db.AbortTransaction();
            Trancount = 0;
        }

        /// <summary>
        /// Commits the transaction IF (and only if) all transactions have been completed. 
        /// </summary>
        public void CommitTransaction()
        {
            Trancount--;
            if (Trancount == 0) db.CompleteTransaction();
            if (Trancount < 0) Trancount = 0;
        }

        /// <summary>
        /// Queries the database for the specified object at ID (stores that object in the cache) and returns the value of the specified property while performing null checks along the way.
        /// </summary>
        /// <typeparam name="T">Type of database object to query</typeparam>
        /// <typeparam name="TReturn">Data type to return </typeparam>
        /// <param name="ID">ID of the record (only works with tables with a primary key)</param>
        /// <param name="PropName">Case insensitive property name to return.</param>
        /// <returns>The value of the property specified or the default value for the return type.</returns>
        public TReturn GetValueByID<T, TReturn>(int? ID, string PropName)
        {
            if (!ID.HasValue) return default(TReturn);
            string cachekey = typeof(T).Name + ID.Value.ToString();

            if (!_cache.ContainsKey(cachekey))
            {
                if (!IsMemoryFull())
                    _cache.Add(cachekey, db.SingleOrDefault<T>(ID.Value));
                else
                    return db.SingleOrDefault<T>(ID.Value).GetPropertyValue<T, TReturn>(PropName);
            }
            T obj = (T)_cache[cachekey];
            return obj.GetPropertyValue<T, TReturn>(PropName);
        }

        public void DeleteEntry<T>(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return;
            string cachekey = typeof(T).Name + ID;

            if (_cache.ContainsKey(cachekey))
            {
                _cache.Remove(cachekey);
            }
        }

        public T GetSingleBySQL<T>(Sql sql, string KeyName)
        {
            KeyName = typeof(T).Name + KeyName;
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                    _cache.Add(KeyName, db.SingleOrDefault<T>(sql));
                else
                    db.SingleOrDefault<T>(sql);
            }

            return (T)_cache[KeyName];
        }

        public T GetCachedObjectByID<T>(int? ID)
        {
            if (!ID.HasValue) return default(T);
            string cachekey = typeof(T).Name + ID.Value.ToString();

            if (!_cache.ContainsKey(cachekey))
            {
                if (!IsMemoryFull())
                    _cache.Add(cachekey, db.SingleOrDefault<T>(ID.Value));
                else
                    return db.SingleOrDefault<T>(ID.Value);
            }
            return (T)_cache[cachekey];
        }

        public List<T> Load<T>(Sql sql)
        {
            if (IsMemoryFull()) throw new OutOfMemoryException("No memory to store objects");
            var lst = db.Fetch<T>(sql);

            var pk = typeof(T).GetCustomAttributes(true).Where(attr => attr is PrimaryKeyAttribute).FirstOrDefault();
            PropertyInfo pi;
            if (pk != null)
            {
                string propname = (pk as PrimaryKeyAttribute).Value;
                pi = typeof(T).GetProperties().Where(prop => prop.Name.ToUpper() == propname.ToUpper()).FirstOrDefault();

            }
            else
            {
                throw new Exception(Properties.Resources.KEY_NOT_FOUND);
            }
            foreach (var item in lst)
            {
                string cachekey = typeof(T).Name + pi.GetValue(item).ToString();
                if (!_cache.ContainsKey(cachekey))
                {
                    _cache.Add(cachekey, item);
                }
            }

            return lst;
        }

        public List<T> GetListBySQL<T>(Sql sql, string KeyName)
        {

            KeyName = typeof(List<T>).Name + KeyName;
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                    _cache.Add(KeyName, db.Fetch<T>(sql));
                else
                    return db.Fetch<T>(sql);
            }
            return (List<T>)_cache[KeyName];
        }

        public List<T> GetListBySproc<T>(string SprocName, string KeyName, object Parameters)
        {
            KeyName = typeof(List<T>).Name + KeyName;
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                    _cache.Add(KeyName, db.FetchProc<T>(SprocName, Parameters));
                else
                    return db.FetchProc<T>(SprocName, Parameters);
            }
            return (List<T>)_cache[KeyName];
        }

        public T GetValueBySproc<T>(string SprocName, string KeyName, object Parameters)
        {
            KeyName = typeof(T).Name + KeyName;
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                    _cache.Add(KeyName, db.FetchProc<T>(SprocName, Parameters));
                else
                    return db.ExecuteScalarProc<T>(SprocName, Parameters);
            }
            return (T)_cache[KeyName];
        }

        private List<SqlParameter> ConvertSprocParams(string SprocName, params object[] Parameters)
        {
            string[] cols = new string[1];
            if (_cache.ContainsKey($"SPROCCOLUMNS_{SprocName}"))
                cols = (string[])_cache[$"SPROCCOLUMNS_{SprocName}"];
            else
            {
                var t = db.FetchProc<ColumnQuery>("sp_sproc_columns", new { procedure_name = SprocName });
                cols = t.Skip(1).Select(s => s.COLUMN_NAME).ToArray();

            }
            if (cols.Length != Parameters.Length) throw new Exception("Parameter count doesn't match sproc parameter count");
            List<SqlParameter> ps = new List<SqlParameter>();
            int idx = 0;
            foreach (var item in cols)
            {
                ps.Add(new SqlParameter(item, Parameters[idx]));
                idx++;
            }
            return ps;
        }

        public List<T> GetListBySproc<T>(string SprocName, string KeyName, params object[] Parameters)
        {
            KeyName = typeof(List<T>).Name + KeyName;
            SprocName = SprocName.Replace("[", "").Replace("]", "");
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                {

                    var rslts = db.FetchProc<T>(SprocName, ConvertSprocParams(SprocName, Parameters));
                    _cache.Add(KeyName, rslts);
                }
                else
                    return db.FetchProc<T>(SprocName, Parameters);
            }
            return (List<T>)_cache[KeyName];
        }

        public T GetValueBySproc<T>(string SprocName, string KeyName, params object[] Parameters)
        {
            KeyName = typeof(T).Name + KeyName;
            SprocName = SprocName.Replace("[", "").Replace("]", "");
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                {

                    var rslts = db.ExecuteScalarProc<T>(SprocName, ConvertSprocParams(SprocName, Parameters));
                    _cache.Add(KeyName, rslts);
                }
                else
                    return db.ExecuteScalarProc<T>(SprocName, Parameters);
            }
            return (T)_cache[KeyName];
        }

        private class ColumnQuery
        {
            public string COLUMN_NAME { get; set; }
        }

        public List<T> GetListBySprocDict<T>(string SprocName, string KeyName, Dictionary<string, object> Parameters)
        {
            KeyName = typeof(List<T>).Name + KeyName;
            if (!_cache.ContainsKey(KeyName))
            {
                if (!IsMemoryFull())
                {
                    List<SqlParameter> ps = new List<SqlParameter>();
                    foreach (var item in Parameters)
                    {
                        ps.Add(new SqlParameter(item.Key, item.Value));
                    }

                    _cache.Add(KeyName, db.FetchProc<T>(SprocName, ps));
                }
                else
                    return db.FetchProc<T>(SprocName, Parameters);
            }
            return (List<T>)_cache[KeyName];
        }

        public bool Exists<T>(string Key)
        {
            Key = typeof(T).Name + Key;
            return _cache.ContainsKey(Key);
        }

        public T Get<T>(string Key)
        {
            var KeyName = typeof(T).Name + Key;
            if (!Exists<T>(Key))
                return default(T);
            else
                return (T)_cache[KeyName];
        }

        public void Set(string Key, object Value)
        {
            Key = (Value?.GetType().Name ?? "") + Key;
            _cache[Key] = Value;
        }

        private bool IsMemoryFull()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                if (percent > (double.Parse(Settings.AppSettings.Get("MaxMemoryCap") ?? "99")))
                {
                    Logger.Error($"Memory cap reached {percent}%. Cannot cache more items");
                    return true;
                }
            }

            return false;
        }
    }


}
