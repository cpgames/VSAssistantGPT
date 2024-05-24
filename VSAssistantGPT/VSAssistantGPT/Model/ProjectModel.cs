using System.Collections.Generic;
using Newtonsoft.Json;

namespace cpGames.VSA
{
    public class ProjectModel
    {
        #region Fields
        public string apiKey = "";
        public string selectedAssistant = "";
        public AssistantModel newAssistantTemplate = new();
        [JsonIgnore]
        public List<ThreadModel> threads = new();
        [JsonIgnore]
        public List<AssistantModel> assistants = new();
        [JsonIgnore]
        public List<ToolModel> toolset = new();
        [JsonIgnore]
        public List<FileModel> files = new();
        [JsonIgnore]
        public List<VectorStoreModel> vectorStores = new();
        #endregion
    }
}