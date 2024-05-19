namespace cpGames.VSA
{
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
    public class TaskModel
    {
        #region Fields
        public string name = "New Task";
        public string description = "New Task Description";
        public string assignedBy = "Unassigned";
        public string assignedTo = "Unassigned";
        public string failureReason = "";
        public int order = 0;
        public bool recurring = false;
        public float recurringInterval = 10;
        public TaskStatus status = TaskStatus.NotStarted;
        #endregion
    }
}