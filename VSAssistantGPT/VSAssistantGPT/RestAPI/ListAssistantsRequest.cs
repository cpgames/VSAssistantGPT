namespace cpGames.VSA.RestAPI
{
    public class ListAssistantsRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/assistants";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListAssistantsRequest() { }
        #endregion
    }
}