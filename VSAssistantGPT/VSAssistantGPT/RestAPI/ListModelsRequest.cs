namespace cpGames.VSA.RestAPI
{
    public class ListModelsRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/models";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListModelsRequest() { }
        #endregion
    }
}