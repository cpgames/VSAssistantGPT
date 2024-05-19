namespace cpGames.VSA.RestAPI
{
    public class CreateThreadRequest : Request
    {
        #region Properties
        protected override string Url => "https://api.openai.com/v1/threads";
        #endregion
    }
}