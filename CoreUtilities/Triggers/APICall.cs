using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using StringTokenFormatter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ensur.Core.Utilities.Helpers;
using System.Net;

namespace Ensur.Core.Utilities.Triggers
{
    /// <summary>
    /// DT-01142, DS-70139 Req 1:RB - API Call object.  Uses a Nuget package called Rest Sharp to simplify the call.  
    /// </summary>
    public class APICall
    {
        public APICall()
        {
            Parameters = new Dictionary<string, string>();
            Headers = new Dictionary<string, string>();
            JsonResultQueries = new Dictionary<string, string>();
            FilesToPost = new Dictionary<string, string>();
            CallMethod = "GET";
            ContentType = "JSON";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private NLog.Logger logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }

        /// <summary>
        /// A dictionary of key/value pairs to pass to the API.  Each parameter is checked for Tokenization before being added. See ExecuteCall for more details
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// The body of a request (duh).  Can be an a string of JSON or XML
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// The application content type.  Either JSON or XML.  Defaults to JSON
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// A dictionary of key/value pairs to pass as to the API as headers.  Each parameter is checked for Tokenization before being added.  See ExecuteCall for more details
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// A way to parse out values out of the return JSON.  Return data is added to the output dictionary of the ExecuteCall function.
        /// https://www.newtonsoft.com/json/help/html/SelectToken.htm
        /// </summary>
        public Dictionary<string, string> JsonResultQueries { get; set; }

        /// <summary>
        /// A dictionary of files to send in the format of "FileName.csv", "c:\temp\LogicalPathToFile\ActualFileName.csv"
        /// Also accepts Tokens.
        /// </summary>
        public Dictionary<string, string> FilesToPost { get; set; }

        /// <summary>
        /// The address of the REST api to call
        /// </summary>
        public string RestURL { get; set; }

        /// <summary>
        /// Which method to call (either GET or POST)
        /// </summary>
        public string CallMethod { get; set; }

        /// <summary>
        /// Optional request timeout in seconds. Zero or less keeps the HTTP stack's default (100 seconds),
        /// which is too short for long LLM generations on local hardware.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Executes the API call configured by the object.  
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Dictionary<string, string> ExecuteCall(Dictionary<string, string> Data)
        {
            logger.Info("Starting API Call");
            var client = new RestClient(RestURL.FormatDictionary<string>(Data));



            logger.Info(client.BaseUrl.ToString());

            Dictionary<string, string> resultData = new Dictionary<string, string>();

            var request = new RestRequest("", (Method)Enum.Parse(typeof(Method), CallMethod));
            if (TimeoutSeconds > 0)
            {
                client.Timeout = TimeoutSeconds * 1000;
                request.Timeout = TimeoutSeconds * 1000;
            }

            if (ContentType.ToUpper() == "XML")
            {
                request.RequestFormat = DataFormat.Xml;
            }
            else
                request.RequestFormat = DataFormat.Json;



            if (Parameters != null && Parameters.Count > 0)
            {
                foreach (var Param in Parameters)
                {
                    request.AddParameter(Param.Key, Param.Value.FormatDictionary<string>(Data));
                }


            }

            if (Headers != null && Headers.Count > 0)
            {
                foreach (var header in Headers)
                {
                    request.AddHeader(header.Key, header.Value.FormatDictionary<string>(Data));
                }

            }



            //Option to upload more than one file at a time.
            //Pass in a data item with the format WantedFileName.doc|c:\pathtoFile\ActualFileName.doc,SecondWantedFileName.doc|c:\pathtofile\SecondActualFilename.doc
            if (Data.ContainsKey("FilesToPost"))
            {
                var splt = Data["FilesToPost"].Split(',');
                foreach (var file in splt)
                {
                    var fileNameSplit = file.Split('|');
                    request.Files.Add(new FileParameter
                    {
                        Name = fileNameSplit[0],
                        Writer = (s) =>
                        {
                            var stream = System.IO.File.OpenRead(fileNameSplit[1]);
                            stream.CopyTo(s);
                            stream.Dispose();
                        },
                        FileName = fileNameSplit[0],

                    });
                }

            }

            if (FilesToPost != null && FilesToPost.Count > 0)
            {
                foreach (var file in FilesToPost)
                {
                    var fi = new System.IO.FileInfo(file.Value.FormatDictionary<string>(Data));

                    //There is an easier (request.AddFile) way to do this, however it uses the name of the physical file
                    request.Files.Add(new FileParameter
                    {
                        Name = file.Key.FormatDictionary<string>(Data),
                        Writer = (s) =>
                        {
                            var stream = System.IO.File.OpenRead(file.Value.FormatDictionary<string>(Data));
                            stream.CopyTo(s);
                            stream.Dispose();
                        },
                        FileName = file.Key.FormatDictionary<string>(Data)
                        ,
                        ContentLength = fi.Length

                    });
                    //request.AddFile(file.Key.FormatDictionary<string>(Data), file.Value.FormatDictionary<string>(Data));
                }
            }


            logger.InfoObj(request.Parameters, "Parameters, headers and request body");

            if (RequestBody != null)
            {
                RequestBody = RequestBody.FormatDictionary<string>(Data);
                if (request.RequestFormat == DataFormat.Xml)
                    request.AddXmlBody(RequestBody);
                else
                    request.AddJsonBody(RequestBody);


                logger.Info("Request body: " + request.Parameters.Where(p => p.Type == ParameterType.RequestBody).First().Value);

            }



            var rslts = client.Execute(request);

            logger.Info($"Execution complete: Success - {rslts.IsSuccessful},  Status Code - {rslts.StatusCode}, Content - {rslts.Content}, error - {rslts.ErrorMessage} ");

            if (!rslts.IsSuccessful)
            {
                throw new Exception($"Error submitting API request: Status Code - {rslts.StatusCode}, Content - {rslts.Content}, error - {rslts.ErrorMessage} ", rslts.ErrorException);

            }


            if (rslts != null && rslts.IsSuccessful && rslts.Content.Length > 0)
            {
                resultData.Add("RESULT", rslts.Content);
                if (rslts.ContentType.ToUpper().Trim().Contains("JSON"))
                {
                    if (JsonResultQueries.Count > 0)
                    {
                        JToken obj;
                        if (rslts.Content.First() == '[')
                        {
                            obj = JArray.Parse(rslts.Content);
                        }
                        else
                        {
                            obj = JObject.Parse(rslts.Content);
                        }
                        foreach (var jquery in JsonResultQueries)
                        {
                            JToken val = obj.SelectToken(jquery.Value);

                            //string val = (string)obj.SelectToken(jquery.Value);
                            resultData[jquery.Key] = val.ToString();
                        }
                    }
                }
            }

            return resultData;

        }


        /// <summary>
        /// Creates an APICall object from JSON text
        /// </summary>
        /// <param name="Json">string to convert</param>
        /// <returns>APICall object</returns>
        public static APICall FromJSON(string Json)
        {
            return JsonConvert.DeserializeObject<APICall>(Json);
        }

        /// <summary>
        /// Converts an APICall object to a JSON String
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}



