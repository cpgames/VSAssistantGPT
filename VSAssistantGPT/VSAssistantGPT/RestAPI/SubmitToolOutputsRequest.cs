using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.RestAPI
{
    public class SubmitToolOutputsRequest : Request
    {
        #region Fields
        protected readonly string _threadId;
        protected readonly string _runId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/threads/{_threadId}/runs/{_runId}/submit_tool_outputs";
        #endregion

        #region Constructors
        public SubmitToolOutputsRequest(string threadId, string runId, Dictionary<string, string> outputs)
        {
            _threadId = threadId;
            _runId = runId;
            var outputsArray = new JArray();
            foreach (var output in outputs)
            {
                var outputObject = new JObject
                {
                    { "tool_call_id", output.Key },
                    { "output", output.Value }
                };
                outputsArray.Add(outputObject);
            }
            var jsonBody = new
            {
                tool_outputs = outputsArray
            };
            var json = JsonConvert.SerializeObject(jsonBody);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}