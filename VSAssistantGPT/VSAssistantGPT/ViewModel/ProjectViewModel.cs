using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class ProjectViewModel : ViewModel<ProjectModel>
    {
        #region Fields
        private double _progress;
        private string _vectorStoreId = "";
        private string _vectorStoreLoaded = "Not Loaded";
        #endregion

        #region Properties
        public string Name
        {
            get => _model.name;
            set
            {
                if (_model.name != value)
                {
                    _model.name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _model.description;
            set
            {
                if (_model.description != value)
                {
                    _model.description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Team
        {
            get => _model.team;
            set
            {
                if (_model.team != value)
                {
                    _model.team = value;
                    OnPropertyChanged();
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

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VectorStoreId
        {
            get => _vectorStoreId;
            set
            {
                if (_vectorStoreId != value)
                {
                    _vectorStoreId = value;
                    OnPropertyChanged();
                    VectorStoreLoaded = string.IsNullOrEmpty(_vectorStoreId) ? "Not Loaded" : "Loaded";
                }
            }
        }

        public string VectorStoreLoaded
        {
            get => _vectorStoreLoaded;
            set
            {
                if (_vectorStoreLoaded != value)
                {
                    _vectorStoreLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TaskViewModel> Tasks { get; } = new();

        public ThreadViewModel Thread { get; } = new(new ThreadModel());

        public ObservableCollection<AssistantViewModel> Assistants { get; } = new();

        public ObservableCollection<ToolViewModel> Toolset { get; } = new();

        public ObservableCollection<FileViewModel> Files { get; } = new();

        public ObservableCollection<VectorStoreViewModel> VectorStores { get; } = new();
        #endregion

        #region Constructors
        public ProjectViewModel(ProjectModel projectModel) : base(projectModel)
        {
            foreach (var taskModel in _model.tasks)
            {
                var taskViewModel = new TaskViewModel(taskModel);
                Tasks.Add(taskViewModel);
                taskViewModel.RemoveAction += () =>
                {
                    _model.tasks.Remove(taskModel);
                    Tasks.Remove(taskViewModel);
                };
            }
        }
        #endregion

        #region Methods
        public void AddTask(TaskModel task)
        {
            _model.tasks.Add(task);
            var taskViewModel = new TaskViewModel(task);
            Tasks.Add(taskViewModel);
            taskViewModel.RemoveAction += () =>
            {
                _model.tasks.Remove(task);
                Tasks.Remove(taskViewModel);
            };
        }

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

        public void AddVectorStore(VectorStoreModel vectorStore)
        {
            _model.vectorStores.Add(vectorStore);
            var vectorStoreViewModel = new VectorStoreViewModel(vectorStore);
            VectorStores.Add(vectorStoreViewModel);
            vectorStoreViewModel.RemoveAction += () =>
            {
                _model.vectorStores.Remove(vectorStore);
                VectorStores.Remove(vectorStoreViewModel);
            };
        }

        public void Save()
        {
            ProjectUtils.SaveProject(_model);
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
                JArray arguments = tool["arguments"] as JArray;
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
        #endregion
    }
}