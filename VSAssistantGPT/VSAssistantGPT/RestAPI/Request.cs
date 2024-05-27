using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cpGames.VSA.RestAPI
{
    public abstract class Request
    {
        #region Nested type: RequestType
        protected enum RequestType
        {
            Get,
            Post,
            Delete
        }
        #endregion

        #region Fields
        protected HttpContent? _content;
        protected dynamic? _response;
        #endregion

        #region Properties
        protected abstract string Url { get; }
        protected virtual RequestType Type => RequestType.Post;
        public dynamic? Response => _response;
        #endregion

        #region Constructors
        protected Request()
        {
        }
        #endregion

        #region Methods
        public async Task<dynamic> SendAsync()
        {
            if (ProjectUtils.ActiveProject == null)
            {
                throw new Exception("No active project");
            }
            while (ProjectUtils.ActiveProject.Working)
            {
                await Task.Delay(100);
            }
            if (string.IsNullOrEmpty(ProjectUtils.ActiveProject.ApiKey))
            {
                throw new Exception("No API key");
            }
            ProjectUtils.ActiveProject.Working = true;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ProjectUtils.ActiveProject.ApiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
            HttpResponseMessage response;
            switch (Type)
            {
                case RequestType.Get:
                    response = await client.GetAsync(Url);
                    break;
                case RequestType.Post:
                    response = await client.PostAsync(Url, _content);
                    break;
                case RequestType.Delete:
                    response = await client.DeleteAsync(Url);
                    break;
                default: throw new Exception("Invalid request type");
            }
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            _response = JsonConvert.DeserializeObject<dynamic>(responseString);
            ProjectUtils.ActiveProject.Working = false;
            if (_response == null)
            {
                throw new Exception("Failed to parse response");
            }
            return _response;
        }
        #endregion
    }
}