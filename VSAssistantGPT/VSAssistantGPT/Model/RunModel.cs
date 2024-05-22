namespace cpGames.VSA
{
    public enum RunStatus
    {
        queued,
        in_progress,
        requires_action,
        cancelling,
        cancelled,
        failed,
        completed,
        expired
    }
    public class RunModel
    {
        #region Fields
        public string id = "";
        public RunStatus status = RunStatus.queued;
        #endregion
    }
}