using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.RestAPI
{
    public class PostMessageRequest : Request
    {
        #region Fields
        protected readonly string _threadId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/threads/{_threadId}/messages";
        #endregion

        #region Constructors
        public PostMessageRequest(string threadId, string content, List<string> attachments)
        {
            _threadId = threadId;
            var contentObject = new JObject
            {
                ["role"] = "user",
                ["content"] = content
            };
            if (attachments.Count > 0)
            {
                var attachmentArray = new JArray();
                foreach (var attachment in attachments)
                {
                    var toolsObject = new JObject
                    {
                        ["type"] = "file_search"
                    };
                    var tools = new JArray
                    {
                        toolsObject
                    };
                    var attachmentObject = new JObject
                    {
                        ["file_id"] = attachment,
                        ["tools"] = tools
                    };
                    attachmentArray.Add(attachmentObject);
                }
                contentObject["attachments"] = attachmentArray;
            }
            var json = JsonConvert.SerializeObject(contentObject);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}