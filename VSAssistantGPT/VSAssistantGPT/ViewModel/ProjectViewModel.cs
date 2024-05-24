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
        private bool _working;
        private readonly AssistantViewModel _newAssistantTemplateViewModel;
        #endregion

        #region Properties
        public string ApiKey
        {
            get => _model.apiKey;
            set
            {
                if (_model.apiKey != value)
                {
                    _model.apiKey = value;
                    OnPropertyChanged();
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

        public Visibility IsDebugVisible
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
                IsTemplate = true
            };
        }
        #endregion

        #region Methods
        public void AddAssistant(AssistantModel assistant)
        {
            _model.assistants.Add(assistant);
            var assistantViewModel = new AssistantViewModel(assistant);
            Assistants.Add(assistantViewModel);
            assistantViewModel.RemoveAction += () =>
            {
                _model.assistants.Remove(assistant);
                Assistants.Remove(assistantViewModel);
            };
        }

        public void AddTool(ToolModel tool, bool loaded)
        {
            _model.toolset.Add(tool);
            var toolViewModel = new ToolViewModel(tool)
            {
                Modified = !loaded
            };
            Toolset.Add(toolViewModel);
            toolViewModel.RemoveAction += () =>
            {
                _model.toolset.Remove(tool);
                Toolset.Remove(toolViewModel);
            };
        }

        public FileViewModel AddFile(FileModel file)
        {
            _model.files.Add(file);
            var fileViewModel = new FileViewModel(file);
            Files.Add(fileViewModel);
            fileViewModel.RemoveAction += () =>
            {
                _model.files.Remove(file);
                Files.Remove(fileViewModel);
            };
            return fileViewModel;
        }

        public VectorStoreViewModel AddVectorStore(VectorStoreModel vectorStore)
        {
            _model.vectorStores.Add(vectorStore);
            var vectorStoreViewModel = new VectorStoreViewModel(vectorStore);
            VectorStores.Add(vectorStoreViewModel);
            vectorStoreViewModel.RemoveAction += () =>
            {
                _model.vectorStores.Remove(vectorStore);
                VectorStores.Remove(vectorStoreViewModel);
            };
            return vectorStoreViewModel;
        }

        public void Save()
        {
            ProjectUtils.SaveProject();
        }

        public async Task LoadAssistantsAsync()
        {
            if (Toolset.Count == 0)
            {
                LoadToolset();
            }
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

        public void LoadToolset()
        {
            Toolset.Clear();
            var toolset = JArray.Parse(ToolAPI.GetToolset());
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

        public async Task LoadFilesAsync()
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

        public async Task LoadVectorStoresAsync()
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
        #endregion
    }
}