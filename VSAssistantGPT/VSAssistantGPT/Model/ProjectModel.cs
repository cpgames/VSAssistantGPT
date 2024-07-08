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
        public AssistantModel newAssistantTemplate = new();
        public List<FileModel> fileCache = new();
        #endregion
    }
}