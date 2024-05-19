namespace cpGames.VSA.RestAPI
{
    public class ListVectorStoreFilesRequest : Request
    {
        #region Fields
        protected readonly string _vectorStoreId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/vector_stores/{_vectorStoreId}/files";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListVectorStoreFilesRequest(string vectorStoreId)
        {
            _vectorStoreId = vectorStoreId;
        }
        #endregion
    }
}