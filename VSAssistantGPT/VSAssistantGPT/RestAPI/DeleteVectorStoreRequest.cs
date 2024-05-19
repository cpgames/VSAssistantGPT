namespace cpGames.VSA.RestAPI
{
    public class DeleteVectorStoreRequest : Request
    {
        #region Fields
        protected readonly string _vectorStoreId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/vector_stores/{_vectorStoreId}";
        protected override RequestType Type => RequestType.Delete;
        #endregion

        #region Constructors
        public DeleteVectorStoreRequest(string vectorStoreId)
        {
            _vectorStoreId = vectorStoreId;
        }
        #endregion
    }
}