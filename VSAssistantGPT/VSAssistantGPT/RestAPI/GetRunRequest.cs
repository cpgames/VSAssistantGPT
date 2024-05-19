namespace cpGames.VSA.RestAPI
{
    public class GetRunRequest : Request
    {
        #region Fields
        protected readonly string _threadId;
        protected readonly string _runId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/threads/{_threadId}/runs/{_runId}";
        protected override RequestType Type => RequestType.Get;
        #endregion

        #region Constructors
        public GetRunRequest(string threadId, string runId)
        {
            _threadId = threadId;
            _runId = runId;
        }
        #endregion
    }
}