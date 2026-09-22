using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensur.Core.Utilities.Classes;
using Ensur.Core.Utilities.Database;
using Ensur.Core.Utilities.Helpers;
using Newtonsoft.Json;
using NLog;
using PetaPoco;

namespace Ensur.Core.Utilities.Triggers
{
    /// <summary>
    /// Triggers singleton.  Loads events from the database and keeps them in a cache.  Events can call Sprocs, API calls or plugin code.
    /// </summary>
    public sealed class Triggers
    {

        #region "Constructors"
        private Triggers()
        {

        }


        #endregion

        #region "Fields"
        private List<DCS_TRIGGER_EVENT> TriggerList
        {
            get
            {
                List<DCS_TRIGGER_EVENT> triggers = null;

                triggers = SessionCache.Instance.BySQLList<DCS_TRIGGER_EVENT>("SELECT * FROM DCS_TRIGGER_EVENT WHERE ACTIVE = 1", "TRIGGERS");

                return triggers;
            }
        }

        /// <summary>
        /// Current instance of the Triggers class.
        /// </summary>
        public static Triggers Instance { get { return TriggersNested.instance; } }


        #endregion


        #region "Methods"

        /// <summary>
        /// Triggers an event and sets it to run in the background.  (Fire and forget)
        /// </summary>
        /// <param name="triggerType">The trigger type to execute</param>
        /// <param name="triggerData">THe input data to execute</param>
        public void TriggerEventAndForget(TriggerTypes triggerType, Dictionary<string, object> triggerData)
        {
#pragma warning disable 4014
            Task.Run(() =>
            {
                TriggerEvent(triggerType, triggerData);
            }).ConfigureAwait(false);
#pragma warning restore 4014
        }

        private static NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }

        /// <summary>
        /// Checks the event cache and triggers an event if it exists.  
        /// </summary>
        /// <param name="triggerType">The type of event to trigger</param>
        /// <param name="triggerData">The data passed to the event during the trigger</param>
        /// <returns>Simple result object wrapped in a task for asynchonous calls</returns>
        public TriggerResult TriggerEvent(TriggerTypes triggerType, Dictionary<string, object> triggerData)
        {

            //Load the triggers from the cache
            //loop through them
            //Switch according to what type of trigger it is
            //Switch according to if it's Async or not
            var triggers = TriggerList.Where(t => t.EVENT_TYPE_ID == (int)triggerType).GroupBy(trigger => trigger.CHAIN_ID);

            //Final return output
            var result = new TriggerResult()
            {
                Success = true,
                ResultData = new Dictionary<string, string>(),
                ResultMessage = ""
            };

            int? SkipToStep = null;


            foreach (var triggerGroup in triggers)
            {

                foreach (var trigger in triggerGroup.OrderBy(trig=>trig.SEQ))
                {
                    if (SkipToStep.HasValue && SkipToStep.Value != trigger.SEQ) continue;
                    logger.Info($"Triggering event {trigger.EVENT_ID} - {trigger.EVENT_DESCRIPTION}");
                    var callResult = new TriggerResult();

                    if (!String.IsNullOrEmpty(trigger.EVENT_CRITERIA))
                    {

                        try
                        {
                            ExpressionEvaluator evaluator = new ExpressionEvaluator();
                            Dictionary<string, object> data = new Dictionary<string, object>();
                            data.Merge(triggerData);
                            if (result.ResultData != null) data.Merge(result.ResultData);

                            evaluator.Variables = data;

                            TriggerCriteria criteria = TriggerCriteria.FromJSON(trigger.EVENT_CRITERIA);

                            if (!string.IsNullOrEmpty(criteria.Criteria))
                            {
                                var criteriaResult = evaluator.Evaluate<bool>(criteria.Criteria);
                                logger.Info($"Evaluated statement {criteria.Criteria} to {criteriaResult}");
                                if (!criteriaResult)
                                {
                                    logger.Info("Criteria failed");
                                    //Only trigger chains can have terminations of execution
                                    if (triggerGroup.Key.HasValue)
                                    {
                                        if (criteria.TerminateExecution) break;
                                        if (criteria.GotoStepOnFailure.HasValue)
                                        {
                                            SkipToStep = criteria.GotoStepOnFailure.Value;
                                        }
                                    }
                                    continue;
                                }
                                else if (triggerGroup.Key.HasValue && criteria.GotoStepOnSuccess.HasValue)
                                {
                                    logger.Info("Criteria passed and skipping to step");
                                    SkipToStep = criteria.GotoStepOnSuccess.Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error evaluating triggers");
                            return new TriggerResult() { Success = false, ResultMessage = $"Error executing trigger criteria {ex.Message} {ex.StackTrace} {trigger.EVENT_CRITERIA}" };

                        }
                    }
                    logger.Info($"Loading switch info : {trigger.CALL_TYPE_ID}");
                    if (!trigger.CALL_TYPE_ID.HasValue || trigger.CALL_TYPE_ID == 0) throw new Exception($"Invalid call type id in defined trigger trigger id {trigger.EVENT_TYPE_ID} Call type {trigger.CALL_TYPE_ID}");
                    TriggerCallTypes CallType = (TriggerCallTypes)trigger.CALL_TYPE_ID.Value;
                    logger.Info("Getting the time");
                    DateTime invokeTime = DateTime.Now;

                                     
                    logger.Info("Executing step");
                    switch (CallType)
                    {
                        case TriggerCallTypes.StoredProcedure:
                            callResult = ExecuteSP(trigger.EVENT_CALL, triggerData, result.ResultData);
                            break;
                        case TriggerCallTypes.Plugin:
                            callResult = ExecutePlugin(trigger.EVENT_CALL, triggerData, result.ResultData);
                            break;
                        case TriggerCallTypes.APICall:
                            callResult = ExecuteAPICaLL(trigger.EVENT_CALL, triggerData, result.ResultData);
                            break;

                    }
                    logger.Info("Step executed");
                    //Log results
                    try
                    {

                        int? docid = null;
                        int? userid = null;
                        if (triggerData.Any(kvp => kvp.Key.ToUpper() == "DOC_ID" || kvp.Key.ToUpper() == "DOCID"))
                        {
                            int did = 0;
                            if (int.TryParse(triggerData.FirstOrDefault(kvp => kvp.Key.ToUpper() == "DOC_ID" || kvp.Key.ToUpper() == "DOCID").Value.ToString(), out did)) docid = did;
                        }
                        if (triggerData.Any(kvp => kvp.Key.ToUpper() == "USER_ID" || kvp.Key.ToUpper() == "USERID"))
                        {
                            int uid = 0;
                            if (int.TryParse(triggerData.FirstOrDefault(kvp => kvp.Key.ToUpper() == "USER_ID" || kvp.Key.ToUpper() == "USERID").Value.ToString(), out uid)) userid = uid;
                        }

                        logger.Info($"{DateTime.Now} - Event ({trigger.EVENT_ID}) completed - {trigger.EVENT_DESCRIPTION} - Started at {invokeTime} ");


                        if (!callResult.Success)
                        {
                            logger.Error(callResult.ResultError, $"Error on event ID {trigger.EVENT_ID} - {trigger.EVENT_DESCRIPTION} {callResult.ResultMessage}");
                        }

                    }
                    catch (Exception ex)
                    {

                        logger.Error(ex, "Erroring logging results");
                    }

                    result.Success = result.Success && callResult.Success;
                    result.ResultMessage = result.ResultMessage + callResult.ResultMessage ?? "";
                    //Subsequent trigger calls are passed the data retrieved from the previous trigger.  This allows triggers to be chained 
                    //together ex (api results going to a stored procedure)
                    if (callResult.ResultData != null) result.ResultData?.Merge(callResult.ResultData);


                }
            }
            return result;
        }


        public TriggerResult TriggerSequence(List<DCS_TRIGGER_EVENT> EventSequence, Dictionary<string, object> triggerData)
        {

            //Load the triggers from the cache
            //loop through them
            //Switch according to what type of trigger it is
            //Switch according to if it's Async or not
            var triggers = EventSequence.GroupBy(trigger => trigger.CHAIN_ID);

            //Final return output
            var result = new TriggerResult()
            {
                Success = true,
                ResultData = new Dictionary<string, string>(),
                ResultMessage = ""
            };

            int? SkipToStep = null;


            foreach (var triggerGroup in triggers)
            {

                foreach (var trigger in triggerGroup.OrderBy(trig => trig.SEQ))
                {
                    if (SkipToStep.HasValue && SkipToStep.Value != trigger.SEQ) continue;
                    logger.Info($"Triggering event {trigger.EVENT_ID} - {trigger.EVENT_DESCRIPTION}");
                    var callResult = new TriggerResult();

                    if (!String.IsNullOrEmpty(trigger.EVENT_CRITERIA))
                    {

                        try
                        {
                            ExpressionEvaluator evaluator = new ExpressionEvaluator();
                            Dictionary<string, object> data = new Dictionary<string, object>();
                            data.Merge(triggerData);
                            if (result.ResultData != null) data.Merge(result.ResultData);

                            evaluator.Variables = data;

                            TriggerCriteria criteria = TriggerCriteria.FromJSON(trigger.EVENT_CRITERIA);

                            if (!string.IsNullOrEmpty(criteria.Criteria))
                            {
                                var criteriaResult = evaluator.Evaluate<bool>(criteria.Criteria);
                                logger.Info($"Evaluated statement {criteria.Criteria} to {criteriaResult}");
                                if (!criteriaResult)
                                {
                                    logger.Info("Criteria failed");
                                    //Only trigger chains can have terminations of execution
                                    if (triggerGroup.Key.HasValue)
                                    {
                                        if (criteria.TerminateExecution) break;
                                        if (criteria.GotoStepOnFailure.HasValue)
                                        {
                                            SkipToStep = criteria.GotoStepOnFailure.Value;
                                        }
                                    }
                                    continue;
                                }
                                else if (triggerGroup.Key.HasValue && criteria.GotoStepOnSuccess.HasValue)
                                {
                                    logger.Info("Criteria passed and skipping to step");
                                    SkipToStep = criteria.GotoStepOnSuccess.Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error evaluating triggers");
                            return new TriggerResult() { Success = false, ResultMessage = $"Error executing trigger criteria {ex.Message} {ex.StackTrace} {trigger.EVENT_CRITERIA}" };

                        }
                    }
                    logger.Info($"Loading switch info : {trigger.CALL_TYPE_ID}");
                    if (!trigger.CALL_TYPE_ID.HasValue || trigger.CALL_TYPE_ID == 0) throw new Exception($"Invalid call type id in defined trigger trigger id {trigger.EVENT_TYPE_ID} Call type {trigger.CALL_TYPE_ID}");
                    TriggerCallTypes CallType = (TriggerCallTypes)trigger.CALL_TYPE_ID.Value;
                    logger.Info("Getting the time");
                    DateTime invokeTime = DateTime.Now;

                    logger.Info("Executing step");
                    switch (CallType)
                    {
                        case TriggerCallTypes.StoredProcedure:
                            callResult = ExecuteSP(trigger.EVENT_CALL, triggerData, result.ResultData);
                            break;
                        case TriggerCallTypes.Plugin:
                            callResult = ExecutePlugin(trigger.EVENT_CALL, triggerData, result.ResultData);
                            break;
                        case TriggerCallTypes.APICall:
                            callResult = ExecuteAPICaLL(trigger.EVENT_CALL, triggerData, result.ResultData);
                            break;

                    }
                    logger.Info("Step executed");
                    //Log results
                    try
                    {

                        int? docid = null;
                        int? userid = null;
                        if (triggerData.Any(kvp => kvp.Key.ToUpper() == "DOC_ID" || kvp.Key.ToUpper() == "DOCID"))
                        {
                            int did = 0;
                            if (int.TryParse(triggerData.FirstOrDefault(kvp => kvp.Key.ToUpper() == "DOC_ID" || kvp.Key.ToUpper() == "DOCID").Value.ToString(), out did)) docid = did;
                        }
                        if (triggerData.Any(kvp => kvp.Key.ToUpper() == "USER_ID" || kvp.Key.ToUpper() == "USERID"))
                        {
                            int uid = 0;
                            if (int.TryParse(triggerData.FirstOrDefault(kvp => kvp.Key.ToUpper() == "USER_ID" || kvp.Key.ToUpper() == "USERID").Value.ToString(), out uid)) userid = uid;
                        }


                        logger.Info($"{DateTime.Now} - Event ({trigger.EVENT_ID}) completed - {trigger.EVENT_DESCRIPTION} - Started at {invokeTime} ");


                        if (!callResult.Success)
                        {
                            logger.Error(callResult.ResultError, $"Error on event ID {trigger.EVENT_ID} - {trigger.EVENT_DESCRIPTION} {callResult.ResultMessage}");
                        }

                    }
                    catch (Exception ex)
                    {

                        logger.Error(ex, "Erroring logging results");
                    }

                    result.Success = result.Success && callResult.Success;
                    result.ResultMessage = result.ResultMessage + callResult.ResultMessage ?? "";
                    //Subsequent trigger calls are passed the data retrieved from the previous trigger.  This allows triggers to be chained 
                    //together ex (api results going to a stored procedure)
                    if (callResult.ResultData != null) result.ResultData?.Merge(callResult.ResultData);


                }
            }
            return result;
        }

        private TriggerResult ExecutePlugin(string EventCall, Dictionary<string, object> TriggerData, Dictionary<string, string> ExtraData)
        {
            PluginCaller caller = new PluginCaller();

            try
            {

                TriggerData.Merge(ExtraData);
                var reslt = caller.ExecutePlugins(TriggerData, EventCall);

                return new TriggerResult() { Success = true, ResultData = reslt };

            }
            catch (Exception ex)
            {
                return new TriggerResult() { Success = false, ResultMessage = ex.Message + " " + ex.StackTrace, ResultError = ex };
            }
        }

        private TriggerResult ExecuteAPICaLL(string JSONCall, Dictionary<string, object> TriggerData, Dictionary<string, string> ExtraData)
        {
            try
            {
                var call = APICall.FromJSON(JSONCall);
                TriggerData?.ToList().ForEach(kvp => ExtraData[kvp.Key] = kvp.Value.ToString());
                var rslts = call.ExecuteCall(ExtraData);
                return new TriggerResult() { Success = true, ResultData = rslts };
            }
            catch (Exception ex)
            {
                return new TriggerResult() { Success = false, ResultMessage = ex.Message + " " + ex.StackTrace + " " + JSONCall, ResultError = ex };
            }
        }


        /// <summary>
        /// Executes a stored procedure.  Expects the trigger text to be simliar to this: EXEC dbo.[SPROC_GSK_MarketSKU_Calc] @DOC_ID={DocID} or dbo.[SPROC_GSK_MarketSKU_Calc] @DOC_ID={DocID}
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="TriggerData"></param>
        /// <param name="ExtraData"></param>
        /// <returns></returns>
        private TriggerResult ExecuteSP(string SQL, Dictionary<string, object> TriggerData, Dictionary<string, string> ExtraData)
        {
            PocoDB db = new PocoDB();
            try
            {
                SQL = SQL.Trim();
                if (SQL.ToUpper().StartsWith("EXEC ")) SQL = SQL.Substring(5); //Kept for legacy scripts
                logger.Info($"Preparing sproc {SQL}");
                List<SqlParameter> prms = new List<SqlParameter>();
                //Merge together input object and any parameters from previous plugins
                if (SQL.Contains("@"))
                {
                    var myparams = SQL.Replace(",", "").Split('@').Select(s => s.Trim()).ToArray();
                    SQL = myparams[0].Trim();

                    foreach (var param in myparams.Skip(1))
                    {
                        var splt = param.Split("=".ToCharArray());
                        string key = splt[0].Trim();
                        string value = splt[1];


                        if (TriggerData != null) value = value.FormatDict(TriggerData);

                        if (ExtraData != null && ExtraData.Count > 0) value = value.FormatDict(ExtraData);

                        prms.Add(new SqlParameter(key, value));
                        
                    }
                    logger.Info("Sproc params: " + string.Join(",", prms.Select(p => $"{p.ParameterName}={p.Value}").ToArray()));

                }
                //Retrieve any results of the proc as a generic object

                var rslts = db.FetchProc<object>(SQL, prms);
                logger.Info("Sproc executed successfully");
                var tr = new TriggerResult() { Success = true, ResultMessage = "" };
                if (rslts != null && rslts.Count > 0)
                {
                    //rslts is an expandoobject which is basically an IDictionary.  I just have to create an actual dictionary from it and convert it to string, string
                    tr.ResultData = new Dictionary<string, object>((IDictionary<string, object>)rslts.First()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString());
                }
                return tr;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error executing SP triggers {db.LastSQL} {db.LastArgs}");
                return new TriggerResult() { Success = false, ResultMessage = ex.Message + " " + ex.StackTrace + " " + db.LastSQL, ResultError = ex };
            }


        }

        #endregion


        #region "Private classes"



        private class TriggersNested
        {



            static TriggersNested()
            {

            }



            internal static readonly Triggers instance = new Triggers();
        }

        /// <summary>
        /// The result of a TriggerEvent call
        /// </summary>
        public class TriggerResult
        {
            /// <summary>
            /// Indicates that the event triggered successfully
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// A simple string output from the result
            /// </summary>
            public string ResultMessage { get; set; }

            public Exception ResultError { get; set; } = null;

            public Dictionary<string, string> ResultData = null;
        }
        
        /// <summary>
        /// Criteria to trigger a step and what happens after the criteria passes/fails
        /// </summary>
        public class TriggerCriteria
        {
            /// <summary>
            /// The criteria to check
            /// </summary>
            public string Criteria { get; set; }

            /// <summary>
            /// If the criteria fails, go to the next step (null) or skip the the supplied event id.  Event ID MUST be in the chain and have a larger sequence.
            /// </summary>
            public int? GotoStepOnFailure { get; set; } = null;

            /// <summary>
            /// If the criteria fails, stop all execution of this chain
            /// </summary>
            public bool TerminateExecution { get; set; } = false;

            /// <summary>
            /// If the criteria succeeds, execute the step and then go to the next step (null) or skip the the supplied event id.  Event ID MUST be in the chain and have a larger sequence.
            /// </summary>
            public int? GotoStepOnSuccess { get; set; } = null;

            /// <summary>
            /// Creates a TriggerCriteria object from a JSON string.
            /// </summary>
            /// <param name="JSONString"></param>
            /// <returns></returns>
            public static TriggerCriteria FromJSON(string JSONString)
            {

                TriggerCriteria tc = new TriggerCriteria();


                if (!String.IsNullOrEmpty(JSONString) && JSONString.Trim().First() == '{')
                {
                    tc =  JsonConvert.DeserializeObject<TriggerCriteria>(JSONString);
                }
                else
                {
                    tc.Criteria = JSONString;
                }

                return tc;
            }

            public string ToJSON()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        #endregion 
    }
}
