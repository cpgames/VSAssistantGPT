using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace cpGames.VSA.RestAPI
{
    public class CreateVectorStoreFileBatchRequest : Request
    {
        #region Fields
        protected readonly string _vectorStoreId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/vector_stores/{_vectorStoreId}/file_batches";
        #endregion

        #region Constructors
        public CreateVectorStoreFileBatchRequest(string vectorStoreId, string[] fileIds)
        {
            _vectorStoreId = vectorStoreId;
            var jsonBody = new
            {
                file_ids = fileIds
            };
            var json = JsonConvert.SerializeObject(jsonBody);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}