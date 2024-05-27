using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class ProjectViewModel : ViewModel<ProjectModel>
    {
        #region Fields
        private bool _modified;
        private bool _working;
        private readonly AssistantViewModel _newAssistantTemplateViewModel;
        #endregion

        #region Properties
        public bool FTE
        {
            get => _model.fte;
            set
            {
                if (_model.fte != value)
                {
                    _model.fte = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Modified
        {
            get => _modified;
            set
            {
                if (_modified != value)
                {
                    _modified = value;
                    OnPropertyChanged();
                    FTE = false;
                }
            }
        }

        public string ApiKey
        {
            get => _model.apiKey;
            set
            {
                if (_model.apiKey != value)
                {
                    _model.apiKey = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public string PythonDll
        {
            get => _model.pythonDll;
            set
            {
                if (_model.pythonDll != value)
                {
                    _model.pythonDll = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public string SelectedAssistant
        {
            get => _model.selectedAssistant;
            set
            {
                if (_model.selectedAssistant != value)
                {
                    _model.selectedAssistant = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public bool Working
        {
            get => _working;
            set
            {
                if (_working != value)
                {
                    _working = value;
                    OnPropertyChanged();
                }
            }
        }

        // hide testing features in release mode
        public Visibility IsTestingVisible
        {
            get
            {
#if DEBUG
                return Visibility.Visible;
#else
                return Visibility.Collapsed;
#endif
            }
        }

        public ThreadViewModel Thread { get; } = new(new ThreadModel());

        public AssistantViewModel NewAssistantTemplateViewModel => _newAssistantTemplateViewModel;

        public ObservableCollection<AssistantViewModel> Assistants { get; } = new();

        public ObservableCollection<ToolViewModel> Toolset { get; } = new();

        public ObservableCollection<FileViewModel> Files { get; } = new();

        public ObservableCollection<VectorStoreViewModel> VectorStores { get; } = new();
        #endregion

        #region Constructors
        public ProjectViewModel(ProjectModel projectModel) : base(projectModel)
        {
            _newAssistantTemplateViewModel = new AssistantViewModel(_model.newAssistantTemplate)
            {
                IsTemplate = true,
                Modified = false
            };
            _newAssistantTemplateViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AssistantViewModel.Modified))
                {
                    Modified |= _newAssistantTemplateViewModel.Modified;
                }
            };
        }
        #endregion

        #region Methods
        public void AddAssistant(AssistantModel assistant)
        {
            var assistantViewModel = new AssistantViewModel(assistant);
            Assistants.Add(assistantViewModel);
            assistantViewModel.RemoveAction += () =>
            {
                Assistants.Remove(assistantViewModel);
            };
        }

        public void AddTool(ToolModel tool, bool loaded)
        {
            var toolViewModel = new ToolViewModel(tool)
            {
                Modified = !loaded
            };
            Toolset.Add(toolViewModel);
            toolViewModel.RemoveAction += () =>
            {
                Toolset.Remove(toolViewModel);
            };
        }

        public void CreateTool()
        {
            if (!ValidateSettings())
            {
                return;
            }
            if (Toolset.Count == 0)
            {
                if (!LoadToolset())
                {
                    return;
                }
            }
            var toolName = "NewTool";
            var index = 1;
            while (Toolset.Any(t => t.Name == toolName))
            {
                toolName = $"NewTool{index++}";
            }
            var tool = new ToolModel
            {
                name = toolName,
                description = "Write tool description (used by GPT)",
                category = "Custom"
            };
            AddTool(tool, false);
        }

        public FileViewModel AddFile(FileModel file)
        {
            var fileViewModel = new FileViewModel(file);
            Files.Add(fileViewModel);
            fileViewModel.RemoveAction += () =>
            {
                Files.Remove(fileViewModel);
            };
            return fileViewModel;
        }

        public VectorStoreViewModel AddVectorStore(VectorStoreModel vectorStore)
        {
            var vectorStoreViewModel = new VectorStoreViewModel(vectorStore);
            VectorStores.Add(vectorStoreViewModel);
            vectorStoreViewModel.RemoveAction += () =>
            {
                VectorStores.Remove(vectorStoreViewModel);
            };
            return vectorStoreViewModel;
        }

        public void Save()
        {
            try
            {
                ProjectUtils.SaveProject();
                _newAssistantTemplateViewModel.Modified = false;
                Modified = false;
                OutputWindowHelper.LogInfo("Info", "Settings saved.");
            }
            catch (Exception e)
            {
                OutputWindowHelper.LogError(e);
            }
        }

        public async Task LoadAssistantsAsync()
        {
            if (!ValidateSettings())
            {
                return;
            }
            if (Toolset.Count == 0)
            {
                LoadToolset();
            }

            try
            {
                Assistants.Clear();
                var listAssistantsRequest = new ListAssistantsRequest();
                var listAssistantsResponse = await listAssistantsRequest.SendAsync();
                JArray data = listAssistantsResponse.data;
                foreach (var assistant in data)
                {
                    var assistantModel = new AssistantModel
                    {
                        id = assistant["id"]!.ToString(),
                        name = assistant["name"]!.ToString(),
                        gptModel = assistant["model"]!.ToString(),
                        description = assistant["description"]!.ToString(),
                        instructions = assistant["instructions"]!.ToString()
                    };
                    if (assistant["tools"] is JArray tools)
                    {
                        foreach (var tool in tools)
                        {
                            if (tool["type"]!.ToString() != "function")
                            {
                                continue;
                            }
                            var toolName = tool["function"]!["name"]!.ToString();
                            var toolModel = Toolset.FirstOrDefault(x => x.Name == toolName);
                            var category = toolModel?.Category ?? "";
                            var toolEntry = new ToolEntryModel
                            {
                                name = toolName,
                                category = category
                            };
                            assistantModel.toolset.Add(toolEntry);
                        }
                    }
                    var toolResources = assistant["tool_resources"];
                    var fileSearch = toolResources?["file_search"];
                    if (fileSearch?["vector_store_ids"] is JArray { Count: > 0 } vectorStores)
                    {
                        assistantModel.vectorStoreId = vectorStores[0].ToString();
                    }
                    AddAssistant(assistantModel);
                }
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
            }
        }

        public bool LoadToolset()
        {
            if (!ValidateSettings())
            {
                return false;
            }
            try
            {
                Toolset.Clear();
                if (!ToolAPI.GetToolset(out var toolsetJson))
                {
                    return false;
                }
                var toolset = JArray.Parse(toolsetJson!);
                foreach (var tool in toolset)
                {
                    var toolModel = new ToolModel
                    {
                        name = tool["name"]!.ToString(),
                        description = tool["description"]!.ToString(),
                        category = tool["category"]!.ToString()
                    };
                    var arguments = tool["arguments"] as JArray;
                    if (arguments != null)
                    {
                        foreach (var argument in arguments)
                        {
                            var argumentModel = new ToolModel.Argument
                            {
                                name = argument["name"]!.ToString(),
                                type = argument["type"]!.ToString(),
                                description = argument["description"]!.ToString()
                            };
                            toolModel.arguments.Add(argumentModel);
                        }
                    }
                    AddTool(toolModel, true);
                }
            }
            catch (Exception e)
            {
                OutputWindowHelper.LogError(e);
                return false;
            }
            return true;
        }

        public void ReloadToolset()
        {
            if (!ValidateSettings())
            {
                return;
            }
            try
            {
                ToolAPI.ReloadTools();
            }
            catch (Exception e)
            {
                OutputWindowHelper.LogError(e);
                return;
            }
            LoadToolset();
        }

        public async Task LoadFilesAsync()
        {
            if (!ValidateSettings())
            {
                return;
            }
            try
            {
                Files.Clear();
                var listFilesRequest = new ListFilesRequest();
                var listFilesResponse = await listFilesRequest.SendAsync();
                JArray data = listFilesResponse.data;
                foreach (var file in data)
                {
                    var fileModel = new FileModel
                    {
                        id = file["id"]!.ToString(),
                        name = file["filename"]!.ToString()
                    };
                    AddFile(fileModel);
                }
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
            }
        }

        public async Task LoadVectorStoresAsync()
        {
            if (!ValidateSettings())
            {
                return;
            }
            try
            {
                VectorStores.Clear();
                var listVectorStoresRequest = new ListVectorStoresRequest();
                var listVectorStoresResponse = await listVectorStoresRequest.SendAsync();
                JArray data = listVectorStoresResponse.data;
                foreach (var vectorStore in data)
                {
                    var vectorStoreModel = new VectorStoreModel
                    {
                        id = vectorStore["id"]!.ToString()
                    };
                    AddVectorStore(vectorStoreModel);
                }
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
            }
        }

        public async Task UploadFilesAsync()
        {
            foreach (var file in Files)
            {
                if (string.IsNullOrEmpty(file.Id))
                {
                    await file.UploadAsync();
                }
            }
        }

        public async Task DeleteSelectedFilesAsync()
        {
            while (Files.Any(file => file.Selected))
            {
                var file = Files.First(f => f.Selected);
                await file.DeleteAsync();
            }
        }

        public async Task WaitForWorkingAsync()
        {
            while (Working)
            {
                await Task.Delay(100);
            }
        }

        public bool ValidateSettings()
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                OutputWindowHelper.LogError("API key missing.");
                return false;
            }
            if (string.IsNullOrEmpty(PythonDll))
            {
                OutputWindowHelper.LogError("Python DLL missing.");
                return false;
            }
            return true;
        }
        #endregion
    }
}