using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

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
        public CreateRunRequest(string threadId, string assistantId)
        {
            _threadId = threadId;
            _assistantId = assistantId;
            var jsonBody = new
            {
                assistant_id = _assistantId
            };
            var json = JsonConvert.SerializeObject(jsonBody);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}