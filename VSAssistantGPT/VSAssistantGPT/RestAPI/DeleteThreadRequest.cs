namespace cpGames.VSA.RestAPI
{
    public class DeleteThreadRequest : Request
    {
        #region Fields
        private readonly string _threadId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/threads/{_threadId}";
        protected override RequestType Type => RequestType.Delete;
        #endregion

        #region Constructors
        public DeleteThreadRequest(string threadId)
        {
            _threadId = threadId;
        }
        #endregion
    }
}