using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using cpGames.VSA.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.RestAPI
{
    public class CreateOrModifyAssistantRequest : Request
    {
        #region Fields
        private readonly string? _assistantId;
        #endregion

        #region Properties
        protected override string Url => string.IsNullOrEmpty(_assistantId) ?
            "https://api.openai.com/v1/assistants" :
            $"https://api.openai.com/v1/assistants/{_assistantId}";
        #endregion

        #region Constructors
        public CreateOrModifyAssistantRequest(AssistantModel assistantModel, IEnumerable<ToolViewModel> toolset)
        {
            _assistantId = assistantModel.id;
            var assistantObject = new JObject
            {
                { "model", assistantModel.gptModel },
                { "name", assistantModel.name },
                { "instructions", assistantModel.instructions },
                { "description", assistantModel.description }
            };
            var toolsArr = new JArray();
            JArray vectorStoresArr = new JArray
            {
                ProjectUtils.ActiveProject.VectorStoreViewModel!.Id
            };
            var fileSearchObj = new JObject
            {
                { "vector_store_ids", vectorStoresArr }
            };
            var toolResourcesObj = new JObject
            {
                { "file_search", fileSearchObj }
            };
            assistantObject["tool_resources"] = toolResourcesObj;
            toolsArr.Add(new JObject
            {
                { "type", "file_search" }
            });
            foreach (var toolEntry in assistantModel.toolset)
            {
                var tool = toolset.FirstOrDefault(t => t.Name == toolEntry.name);
                if (tool == null)
                {
                    OutputWindowHelper.LogInfo("CreateAssistantRequest", $"Tool {toolEntry} not found in toolset");
                    continue;
                }
                var funcObj = new JObject
                {
                    { "name", tool.Name },
                    { "description", tool.Description }
                };
                if (tool.Model.arguments.Count > 0)
                {
                    var propListObj = new JObject();
                    foreach (var property in tool.Model.arguments)
                    {
                        var propObj = new JObject
                        {
                            { "type", property.type },
                            { "description", property.description }
                        };
                        propListObj[property.name] = propObj;
                    }
                    var requiredArr = new JArray();
                    foreach (var property in tool.Model.arguments
                                 .Where(property => property.required))
                    {
                        requiredArr.Add(property.name);
                    }
                    var paramObj = new JObject
                    {
                        { "type", "object" },
                        { "properties", propListObj },
                        { "required", requiredArr }
                    };
                    funcObj["parameters"] = paramObj;
                }
                var toolObj = new JObject
                {
                    { "type", "function" },
                    { "function", funcObj }
                };
                toolsArr.Add(toolObj);
            }
            assistantObject["tools"] = toolsArr;
            var json = JsonConvert.SerializeObject(assistantObject);
            _content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}