using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace cpGames.VSA.RestAPI
{
    public class CreateVectorStoreFileRequest : Request
    {
        #region Fields
        protected readonly string _vectorStoreId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/vector_stores/{_vectorStoreId}/files";
        #endregion

        #region Constructors
        public CreateVectorStoreFileRequest(string vectorStoreId, string fileId)
        {
            _vectorStoreId = vectorStoreId;
            var jsonBody = new
            {
                file_id = fileId
            };
            var json = JsonConvert.SerializeObject(jsonBody);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}