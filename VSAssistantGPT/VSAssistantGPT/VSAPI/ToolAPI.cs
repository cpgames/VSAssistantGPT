using System;
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
        }
        #endregion

        #region Methods
        private static async Task<bool> InitializePythonEngineAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (PythonEngine.IsInitialized)
            {
                return true;
            }

            try
            {
                if (!ProjectUtils.ActiveProject.ValidateSettings())
                {
                    return false;
                }
                var pythonFile = new FileInfo(ProjectUtils.ActiveProject.PythonDll);
                if (!pythonFile.Exists)
                {
                    throw new Exception("Python dll not found.");
                }
                Runtime.PythonDLL = pythonFile.FullName;
                PythonEngine.PythonHome = pythonFile.DirectoryName!;

                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                RegisterPythonCallbacks();
                await OutputWindowHelper.LogInfoAsync("PythonEngine", "PythonEngine initialized.");
                return true;
            }
            catch (Exception ex)
            {
                await OutputWindowHelper.LogErrorAsync(ex);
            }
            return false;
        }

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

        private static string GetResponseError(string error)
        {
            OutputWindowHelper.LogError(error);
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

            var argumentsDict = (arguments != null && arguments.ToString() != "[]") ?
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
                try
                {
                    var result = method.Invoke(null, new object[] { argumentsDict });
                    return result.ToString();
                }
                catch (Exception e)
                {
                    return GetResponseError($"Error invoking tool '{toolNameStr}': {e.Message}");
                }
            }

            return GetResponseError($"Tool '{toolNameStr}' not found.");
        }

        public static async Task<string> HandleToolCallAsync(JToken toolCall)
        {
            if (!await InitializePythonEngineAsync())
            {
                return GetResponseError("PythonEngine not initialized");
            }
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

        public static async Task<string?> GetToolsetAsync()
        {
            if (!await InitializePythonEngineAsync())
            {
                return null;
            }
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

        public static string OpenTool(string toolName)
        {
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
                var response = tools.open_tool(toolName).ToString();
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

        public static string ReloadTools()
        {
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
                var response = tools.reload_tools().ToString();
                return response;
            }
        }

        public static string HandleToolCall(JToken toolCall)
        {
            var toolCallJson = toolCall.ToString();
            using (Py.GIL())
            {
                dynamic tools = Py.Import("AssistantTools");
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
                selection["relative_path"] = DTEUtils.GetActiveDocumentRelativePath();
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
            DTEUtils.SaveDocument();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject GetActiveDocumentText(Dictionary<string, dynamic> arguments)
        {
            var text = DTEUtils.GetActiveDocumentText();
            return new JObject
            {
                ["text"] = text,
                ["result"] = "success"
            };
        }

        public static JObject SetActiveDocumentText(Dictionary<string, dynamic> arguments)
        {
            DTEUtils.SetActiveDocumentText(arguments["text"]);
            DTEUtils.SaveDocument();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject GetActiveDocumentPath(Dictionary<string, dynamic> arguments)
        {
            var relativePath = DTEUtils.GetActiveDocumentRelativePath();
            return new JObject
            {
                ["relative_path"] = relativePath,
                ["result"] = "success"
            };
        }

        public static JObject GetDocumentText(Dictionary<string, dynamic> arguments)
        {
            var relativePath = arguments["relative_path"];
            if (!DTEUtils.DocumentExists(relativePath))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.OpenDocument(relativePath);
            var result = new JObject
            {
                ["result"] = "success",
                ["text"] = DTEUtils.GetActiveDocumentText()
            };
            return result;
        }

        public static JObject SetDocumentText(Dictionary<string, dynamic> arguments)
        {
            var relativePath = arguments["relative_path"];
            if (!DTEUtils.DocumentExists(relativePath))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.OpenDocument(relativePath);
            DTEUtils.SetActiveDocumentText(arguments["text"]);
            DTEUtils.SaveDocument();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject OpenDocument(Dictionary<string, dynamic> arguments)
        {
            string relativePath = arguments["relative_path"];
            if (!DTEUtils.DocumentExists(relativePath))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.OpenDocument(relativePath);
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject CloseDocument(Dictionary<string, dynamic> arguments)
        {
            string relativePath = arguments["relative_path"];
            if (!DTEUtils.DocumentExists(relativePath))
            {
                return new JObject
                {
                    ["result"] = "file not found"
                };
            }
            DTEUtils.CloseDocument(relativePath);
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject CreateDocument(Dictionary<string, dynamic> arguments)
        {
            string relativePath = arguments["relative_path"];
            string text = arguments["text"];
            DTEUtils.CreateDocument(relativePath);
            SetDocumentText(
                new Dictionary<string, dynamic>
                {
                    ["relative_path"] = relativePath,
                    ["text"] = text
                });
            DTEUtils.SaveDocument();
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject DeleteDocument(Dictionary<string, dynamic> arguments)
        {
            string relativePath = arguments["relative_path"];
            DTEUtils.DeleteDocument(relativePath);
            return new JObject
            {
                ["result"] = "success"
            };
        }

        public static JObject HasDocument(Dictionary<string, dynamic> arguments)
        {
            string relativePath = arguments["relative_path"];
            var result = new JObject
            {
                ["result"] = DTEUtils.DocumentExists(relativePath)
            };
            return result;
        }

        public static JObject ListDocuments(Dictionary<string, dynamic> arguments)
        {
            var files = DTEUtils.GetProjectItemsInActiveProject();
            var filesArr = new JArray();
            foreach (var file in files)
            {
                var relativePath = DTEUtils.GetRelativePath(file.FileNames[0]);
                filesArr.Add(relativePath);
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

        public static JObject GetProjectPath(Dictionary<string, dynamic> arguments)
        {
            var projectPath = DTEUtils.GetProjectPath();
            var result = new JObject
            {
                ["result"] = "success",
                ["project_path"] = projectPath
            };
            return result;
        }
        #endregion
    }
}