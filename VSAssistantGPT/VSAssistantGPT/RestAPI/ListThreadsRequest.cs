namespace cpGames.VSA.RestAPI
{
    public class ListThreadsRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/threads";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListThreadsRequest() { }
        #endregion
    }
}