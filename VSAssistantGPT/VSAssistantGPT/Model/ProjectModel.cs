using System.Collections.Generic;

namespace cpGames.VSA
{
    public class ProjectModel
    {
        #region Fields
        public bool fte = true;
        public string apiKey = "";
        public string pythonDll = "";
        public string selectedAssistant = "";
        public bool sync = true;
        public AssistantModel newAssistantTemplate = new();
        public List<FileModel> files = new();
        #endregion
    }
}