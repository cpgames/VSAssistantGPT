namespace cpGames.VSA.RestAPI
{
    public class ListMessagesRequest : Request
    {
        #region Fields
        private readonly string _threadId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/threads/{_threadId}/messages";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public ListMessagesRequest(string threadId)
        {
            _threadId = threadId;
        }
        #endregion
    }
}