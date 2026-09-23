using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Ensur.Core.Utilities.Helpers;
using Ensur.Core.Utilities.Properties;
using Ensur.Core.Utilities.Settings;
using NLog.Fluent;
using PetaPoco;
using PetaPoco.Core;
using System;
using System.Collections.Generic;
using System.Data;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Database
{
    public static class DatabaseHelpers
    {
        
        /// <summary>
        /// Takes an IEnumerable of objects (Leaning on POCOs from Petapoco here for help) and converts it to a datatable
        /// </summary>
        /// <param name="pocos">Can be a plain object or you can use Petapoco attributes to name the columns</param>
        /// <returns></returns>
        /// <exception cref="UserError"></exception>
        public static System.Data.DataTable ToDataTable(this IEnumerable<object> pocos)
        {
            var log = NLog.LogManager.GetCurrentClassLogger();
            IList<PocoColumn> columns = new List<PocoColumn>();
            PocoData pd;
            object template;
            string tableName;
            string primaryKeyName;
            bool autoIncrement;


            template = pocos.First<object>();

            if (null == template)
                return null;

            var db = Ensur.Core.Utilities.Database.SessionCache.Instance.SharedDBConnection;

            pd = PocoData.ForType(template.GetType(), db.DefaultMapper);
            tableName = pd.TableInfo.TableName;
            primaryKeyName = pd.TableInfo.PrimaryKey;
            autoIncrement = pd.TableInfo.AutoIncrement;
            System.Data.DataTable dt = new System.Data.DataTable(tableName);



            try
            {


                foreach (var i in pd.Columns)
                {
                    dt.Columns.Add(i.Value.ColumnName, Nullable.GetUnderlyingType(i.Value.PropertyInfo.PropertyType) ?? i.Value.PropertyInfo.PropertyType);
                }

                DataRow nr = null;
                object val = null;
                foreach (object poco in pocos)
                {
                    nr = dt.NewRow();
                    foreach (var i in pd.Columns)
                    {
                        val = i.Value.GetValue(poco);
                        if (val != null) nr[i.Value.ColumnName] = val;
                    }
                    dt.Rows.Add(nr);
                }

                return dt;

            }
            catch (Exception x)
            {
                log.Error(x, "Error during bulk insert");
                throw new UserError(Resources.GENERAL_ERROR, "GENERAL_ERROR");
            }
        }


        public static void BulkInsert(this PocoDB db, IEnumerable<object> pocos)
        {
            var log = NLog.LogManager.GetCurrentClassLogger();

            IList<PocoColumn> columns = new List<PocoColumn>();
            PocoData pd;
            object template;
            string tableName;
            string primaryKeyName;
            bool autoIncrement;


            template = pocos.First<object>();

            if (null == template)
                return;

            pd = PocoData.ForType(template.GetType(), db.DefaultMapper);
            tableName = pd.TableInfo.TableName;
            primaryKeyName = pd.TableInfo.PrimaryKey;
            autoIncrement = pd.TableInfo.AutoIncrement;
            System.Data.DataTable dt = new System.Data.DataTable(tableName);


            var BulkCopy = new SqlBulkCopy(db.ConnectionString);

            try
            {

                BulkCopy.DestinationTableName = tableName;

                foreach (var i in pd.Columns)
                {
                    BulkCopy.ColumnMappings.Add(i.Value.ColumnName, i.Value.ColumnName);
                    dt.Columns.Add(i.Value.ColumnName, Nullable.GetUnderlyingType(i.Value.PropertyInfo.PropertyType) ?? i.Value.PropertyInfo.PropertyType);
                }

                DataRow nr = null;
                object val = null;
                foreach (object poco in pocos)
                {
                    nr = dt.NewRow();
                    foreach (var i in pd.Columns)
                    {
                        val = i.Value.GetValue(poco);
                        if (val != null) nr[i.Value.ColumnName] = val;
                    }
                    dt.Rows.Add(nr);
                }

                BulkCopy.WriteToServer(dt);


            }
            catch (Exception x)
            {
                log.Error(x, "Error during bulk insert");
                throw new UserError(Resources.GENERAL_ERROR, "GENERAL_ERROR");
            }
        }

        public static object[] BulkInsertSmall(this PocoDB db, IEnumerable<object> pocos)
        {
            Sql sql;
            IList<PocoColumn> columns = new List<PocoColumn>();
            IList<object> parameters;
            IList<object> inserted;
            PocoData pd;
            Type primaryKeyType;
            object template;
            string commandText;
            string tableName;
            string primaryKeyName;
            bool autoIncrement;


            if (null == pocos)
                return new object[] { };

            template = pocos.First<object>();

            if (null == template)
                return null;

            pd = PocoData.ForType(template.GetType(), db.DefaultMapper);
            tableName = pd.TableInfo.TableName;
            primaryKeyName = pd.TableInfo.PrimaryKey;
            autoIncrement = pd.TableInfo.AutoIncrement;

            try
            {
                db.OpenSharedConnection();
                try
                {
                    var names = new List<string>();
                    var values = new List<string>();
                    var index = 0;
                    foreach (var i in pd.Columns)
                    {
                        // Don't insert result columns
                        if (i.Value.ResultColumn)
                            continue;

                        // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                        if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                        {
                            primaryKeyType = i.Value.PropertyInfo.PropertyType;

                            // Setup auto increment expression
                            string autoIncExpression = db.Provider.GetAutoIncrementExpression(pd.TableInfo);
                            if (autoIncExpression != null)
                            {
                                names.Add(i.Key);
                                values.Add(autoIncExpression);
                            }
                            continue;
                        }

                        names.Add(db.Provider.EscapeSqlIdentifier(i.Key));
                        values.Add(string.Format("{0}{1}", "@", index++));
                        columns.Add(i.Value);
                    }

                    string outputClause = String.Empty;
                    if (autoIncrement)
                    {
                        outputClause = db.Provider.GetInsertOutputClause(primaryKeyName);
                    }

                    commandText = string.Format("INSERT INTO {0} ({1}){2} VALUES",
                                    db.Provider.EscapeTableName(tableName),
                                    string.Join(",", names.ToArray()),
                                    outputClause
                                    );

                    sql = new Sql(commandText);
                    parameters = new List<object>();
                    string valuesText = string.Concat("(", string.Join(",", values.ToArray()), ")");
                    bool isFirstPoco = true;

                    foreach (object poco in pocos)
                    {
                        parameters.Clear();
                        foreach (PocoColumn column in columns)
                        {
                            parameters.Add(column.GetValue(poco));
                        }

                        sql.Append(valuesText, parameters.ToArray<object>());

                        if (isFirstPoco)
                        {
                            valuesText = "," + valuesText;
                            isFirstPoco = false;
                        }
                    }

                    inserted = new List<object>();

                    using (var cmd = db.CreateCommand(db.Connection, sql.SQL, sql.Arguments))
                    {
                        if (!autoIncrement)
                        {
                            //parameters.Clear();
                            cmd.ExecuteNonQuery();
                            db.OnExecutedCommand(cmd);

                            PocoColumn pkColumn;
                            if (primaryKeyName != null && pd.Columns.TryGetValue(primaryKeyName, out pkColumn))
                            {
                                foreach (object poco in pocos)
                                {
                                    inserted.Add(pkColumn.GetValue(poco));
                                }
                            }

                            return inserted.ToArray<object>();
                        }

                        // BUG: the following line reportedly causes duplicate inserts; need to confirm
                        //object id = _dbType.ExecuteInsert(this, cmd, primaryKeyName);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inserted.Add(reader[0]);
                            }
                        }

                        object[] primaryKeys = inserted.ToArray<object>();

                        // Assign the ID back to the primary key property
                        if (primaryKeyName != null)
                        {
                            PocoColumn pc;
                            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                            {
                                index = 0;
                                foreach (object poco in pocos)
                                {
                                    pc.SetValue(poco, pc.ChangeType(primaryKeys[index]));
                                    index++;
                                }
                            }
                        }

                        return primaryKeys;
                    }
                }
                finally
                {
                    db.CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                if (db.OnException(x))
                    throw;
                return null;
            }
        }
    }
}
