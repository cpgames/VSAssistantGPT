using System.IO;
using System.Net.Http;

namespace cpGames.VSA.RestAPI
{
    public class UploadFileRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/files";
        #endregion

        #region Constructors
        public UploadFileRequest(string filePath)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("assistants"), "purpose");
            var fileContent = new StreamContent(File.OpenRead(filePath));
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            _content = content;
        }
        #endregion
    }
}