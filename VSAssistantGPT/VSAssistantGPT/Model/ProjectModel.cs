using System.Collections.Generic;

namespace cpGames.VSA
{
    public class ProjectModel
    {
        #region Fields
        public string name = "New Project";
        public string description = "New Project Description";
        public string team = "";
        public string apiKey = "";
        public string selectedAssistant = "";
        public List<ThreadModel> threads = new();
        public List<AssistantModel> assistants = new();
        public List<ToolModel> toolset = new();
        public List<FileModel> files = new();
        public List<VectorStoreModel> vectorStores = new();
        #endregion
    }
}