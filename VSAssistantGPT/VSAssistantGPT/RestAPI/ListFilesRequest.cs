namespace cpGames.VSA.RestAPI
{
    public class ListFilesRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/files";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListFilesRequest() { }
        #endregion
    }
}