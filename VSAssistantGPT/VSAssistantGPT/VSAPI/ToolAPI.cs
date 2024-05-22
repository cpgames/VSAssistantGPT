using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Python.Runtime;

namespace cpGames.VSA
{
    public delegate Task<string> ToolCallback(string funcJson);
    public static class ToolAPI
    {
        #region Constructors
        static ToolAPI()
        {
            var pythonDir = Utils.GetOrCreateAppDir("Tools");
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var pythonResourceDir = Path.Combine(Path.GetDirectoryName(assemblyPath)!, "Resources", "python");
            foreach (var file in Directory.GetFiles(pythonResourceDir)
                         .Where(x => x.EndsWith(".py")))
            {
                var destFile = Path.Combine(pythonDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            Runtime.PythonDLL = "python39.dll";
            PythonEngine.PythonHome = @"C:\Users\mikha\miniconda3";

            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            RegisterPythonCallbacks();
        }
        #endregion

        #region Methods
        private static void RegisterPythonCallbacks()
        {
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(Utils.GetOrCreateAppDir("Tools"));

                dynamic tools = Py.Import("AssistantTools");
                tools.register_tool_callback(new ToolCallback(InvokeToolFunctionAsync));
            }
        }

        private static string GetResponseSuccess()
        {
            JToken response = new JObject
            {
                ["success"] = true
            };
            return response.ToString();
        }

        private static string GetResponseError(string error)
        {
            JToken response = new JObject
            {
                ["success"] = false,
                ["error"] = error
            };
            return response.ToString();
        }

        private static async Task<string> InvokeToolFunctionAsync(string funcJson)
        {
            var toolCall = JToken.Parse(funcJson);
            var toolName = toolCall["name"];
            if (toolName is not { Type: JTokenType.String })
            {
                return GetResponseError("Tool name str not found in JSON object.");
            }
            var toolNameStr = toolName.ToString();
            var arguments = toolCall["arguments"];

            var argumentsDict = arguments != null ?
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(arguments.ToString()) :
                new Dictionary<string, dynamic>();
            if (argumentsDict == null)
            {
                return GetResponseError("Failed to parse arguments.");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Use reflection to find the method in the current static class
            var method = typeof(ToolAPI).GetMethod(
                toolNameStr,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Check if the method exists and invoke it if found
            if (method != null)
            {
                var result = method.Invoke(null, new object[] { argumentsDict });
                return result.ToString();
            }

            return GetResponseError($"Tool '{toolNameStr}' not found.");
        }

        public static string HandleToolRequest(string name, Dictionary<string, object> arguments)
        {
            // Implement your tool logic here
            return $"Received tool request: {name}, with arguments: {JsonConvert.SerializeObject(arguments)}";
        }

        public static async Task<string> HandleToolCallAsync(JToken toolCall)
        {
            // Convert tool call to JSON string
            var toolCallJson = toolCall.ToString(Formatting.Indented);
            await OutputWindowHelper.LogInfoAsync("Tool Call", toolCallJson);
            var result = await Task.Run(
                () =>
                {
                    using (Py.GIL())
                    {
                        dynamic sys = Py.Import("sys");
                        sys.path.append(Utils.GetOrCreateAppDir("Tools"));
                        dynamic tools = Py.Import("AssistantTools");

                        // Call the Python function `call_tool_function`
                        string response = tools.call_tool_function(toolCallJson).ToString();
                        return response;
                    }
                });
            await OutputWindowHelper.LogInfoAsync("Tool Result", result);
            return result;
        }

        public static string GetToolset()
        {
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
                var toolset = tools.get_tools_info().ToString();
                return toolset;
            }
        }

        public static string CreateTool(JToken toolJson)
        {
            var toolInfoJson = toolJson.ToString();
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
                var response = tools.create_tool(toolInfoJson).ToString();
                return response;
            }
        }

        public static string SaveTool(string originalName, JToken toolJson)
        {
            var toolInfoJson = toolJson.ToString();
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
                var response = tools.save_tool(originalName, toolInfoJson).ToString();
                return response;
            }
        }

        public static string RemoveTool(string toolName)
        {
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
                var response = tools.remove_tool(toolName).ToString();
                return response;
            }
        }

        public static string HandleToolCall(JToken toolCall)
        {
            var toolCallJson = toolCall.ToString();
            using (Py.GIL())
            {
                // Import the Python script
                dynamic tools = Py.Import("AssistantTools");

                //var response = tools.test_call_func("testFunc").ToString();
                var response = tools.call_tool_function(toolCallJson).ToString();
                return response;
            }
        }

        public static JObject HasSelection(Dictionary<string, dynamic> arguments)
        {
            var hasSelection = new JObject();
            hasSelection["result"] = VSATagger.View?.Selection.IsEmpty == false;
            return hasSelection;
        }

        public static JObject GetSelection(Dictionary<string, dynamic> arguments)
        {
            var selection = new JObject();
            if (!DTEUtils.IsSelectionEmpty())
            {
                selection["relativePath"] = DTEUtils.GetActiveDocumentRelativePath();
                var selectionText = DTEUtils.GetSelection();
                selection["text"] = selectionText;
                selection["result"] = !string.IsNullOrEmpty(selectionText) ?
                    "success" :
                    "selection is empty";
            }
            else
            {
                selection["path"] = "";
                selection["text"] = "";
                selection["result"] = "no active document";
            }
            return selection;
        }

        public static JObject SetSelection(Dictionary<string, dynamic> arguments)
        {
            DTEUtils.SetSelection(arguments["text"]);
            DTEUtils.SaveFile();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject GetFileText(Dictionary<string, dynamic> arguments)
        {
            var filename = arguments["filename"];
            if (!DTEUtils.FileExists(filename))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.OpenFile(filename);
            var result = new JObject
            {
                ["result"] = "success",
                ["text"] = DTEUtils.GetActiveDocumentText()
            };
            return result;
        }

        public static JObject SetFileText(Dictionary<string, dynamic> arguments)
        {
            var filename = arguments["filename"];
            if (!DTEUtils.FileExists(filename))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.OpenFile(filename);
            DTEUtils.SetActiveDocumentText(arguments["text"]);
            DTEUtils.SaveFile();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject OpenFile(Dictionary<string, dynamic> arguments)
        {
            string filename = arguments["filename"];
            if (!DTEUtils.FileExists(filename))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.OpenFile(filename);
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject CreateFile(Dictionary<string, dynamic> arguments)
        {
            string filename = arguments["filename"];
            string text = arguments["text"];
            DTEUtils.CreateFile(filename);
            SetFileText(
                new Dictionary<string, dynamic>
                {
                    ["filename"] = filename,
                    ["text"] = text
                });
            DTEUtils.SaveFile();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject DeleteFile(Dictionary<string, dynamic> arguments)
        {
            string filename = arguments["filename"];
            DTEUtils.DeleteFile(filename);
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject HasFile(Dictionary<string, dynamic> arguments)
        {
            string filename = arguments["filename"];
            var result = new JObject
            {
                ["result"] = DTEUtils.FileExists(filename)
            };
            return result;
        }

        public static JObject ListFiles(Dictionary<string, dynamic> arguments)
        {
            var files = DTEUtils.GetProjectItemsInActiveProject();
            var filesArr = new JArray();
            foreach (var file in files)
            {
                var filename = DTEUtils.GetRelativePath(file.FileNames[0]);
                filesArr.Add(filename);
            }
            var result = new JObject
            {
                ["result"] = "success",
                ["files"] = filesArr
            };
            return result;
        }

        public static JArray GetErrors(Dictionary<string, dynamic> arguments)
        {
            return DTEUtils.GetErrors();
        }
        #endregion
    }
}