using System.Collections.Generic;

namespace cpGames.VSA
{
    public class ToolModel
    {
        #region Nested type: Property
        public class Argument
        {
            #region Fields
            public string name = "";
            public string type = "";
            public string description = "";
            public bool required = true;
            #endregion
        }
        #endregion

        #region Fields
        public string name = "NewTool";
        public string category = "New Category";
        public string description = "New Description";
        public List<Argument> arguments = new();
        #endregion
    }
}