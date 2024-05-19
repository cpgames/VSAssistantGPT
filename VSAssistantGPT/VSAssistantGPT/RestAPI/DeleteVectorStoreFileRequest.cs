namespace cpGames.VSA.RestAPI
{
    public class DeleteVectorStoreFileRequest : Request
    {
        #region Fields
        protected readonly string _vectorStoreId;
        private readonly string _fileId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/vector_stores/{_vectorStoreId}/files/{_fileId}";
        protected override RequestType Type => RequestType.Delete;
        #endregion

        #region Constructors
        public DeleteVectorStoreFileRequest(string vectorStoreId, string fileId)
        {
            _vectorStoreId = vectorStoreId;
            _fileId = fileId;
        }
        #endregion
    }
}