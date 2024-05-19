namespace cpGames.VSA.RestAPI
{
    public class DeleteFileRequest : Request
    {
        #region Fields
        private readonly string _fileId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/files/{_fileId}";
        protected override RequestType Type => RequestType.Delete;
        #endregion

        #region Constructors
        public DeleteFileRequest(string fileId)
        {
            _fileId = fileId;
        }
        #endregion
    }
}