namespace cpGames.VSA.RestAPI
{
    public class DeleteAssistantRequest : Request
    {
        #region Fields
        private readonly string _assistantId;
        #endregion

        #region Properties
        protected override string Url => $"https://api.openai.com/v1/assistants/{_assistantId}";
        protected override RequestType Type => RequestType.Delete;
        #endregion

        #region Constructors
        public DeleteAssistantRequest(string assistantId)
        {
            _assistantId = assistantId;
        }
        #endregion
    }
}