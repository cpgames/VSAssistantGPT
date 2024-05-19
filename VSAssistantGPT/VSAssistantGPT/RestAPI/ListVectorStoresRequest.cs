namespace cpGames.VSA.RestAPI
{
    public class ListVectorStoresRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/vector_stores";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListVectorStoresRequest() { }
        #endregion
    }
}