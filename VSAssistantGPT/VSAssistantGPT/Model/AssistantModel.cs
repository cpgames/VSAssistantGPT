using System.Collections.Generic;

namespace cpGames.VSA
{
    public class AssistantModel
    {
        #region Fields
        public string id = "";
        public string name = "";
        public string gptModel = "";
        public string description = "";
        public string instructions = "";
        public List<ToolEntryModel> toolset = new();
        #endregion
    }
}