using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.RestAPI
{
    public class CreateRunRequest : Request
    {
        #region Fields
        protected readonly string _threadId;
        protected readonly string _assistantId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/threads/{_threadId}/runs";
        #endregion

        #region Constructors
        public CreateRunRequest(ThreadModel thread, AssistantModel assistant)
        {
            _threadId = thread.id;
            _assistantId = assistant.id;
            var runObject = new JObject
            {
                { "assistant_id", _assistantId }
            };
            var json = JsonConvert.SerializeObject(runObject);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}