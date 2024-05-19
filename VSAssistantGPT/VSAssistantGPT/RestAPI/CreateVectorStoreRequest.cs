namespace cpGames.VSA.RestAPI
{
    public class CreateVectorStoreRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/vector_stores";
        #endregion

        #region Constructors
        public CreateVectorStoreRequest() { }
        #endregion
    }
}