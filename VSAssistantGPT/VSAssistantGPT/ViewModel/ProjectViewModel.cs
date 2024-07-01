using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using cpGames.VSA.RestAPI;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using Constants = EnvDTE.Constants;
using DocumentEvents = EnvDTE.DocumentEvents;
using ProjectItemsEvents = EnvDTE.ProjectItemsEvents;
using SolutionEvents = EnvDTE.SolutionEvents;

namespace cpGames.VSA.ViewModel
{
    public class ProjectViewModel : ViewModel<ProjectModel>
    {
        #region Fields
        private readonly DocumentEvents _documentEvents;
        private readonly ProjectItemsEvents _projectItemsEvents;
        private readonly SolutionEvents _solutionEvents;
        private bool _modified;
        private VectorStoreViewModel? _vectorStoreViewModel;
        private bool _working;
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

        public bool Sync
        {
            get => _model.sync;
            set
            {
                if (_model.sync != value)
                {
                    _model.sync = value;
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

        public VectorStoreViewModel? VectorStoreViewModel
        {
            get => _vectorStoreViewModel;
            set
            {
                if (_vectorStoreViewModel != value)
                {
                    _vectorStoreViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public ThreadViewModel Thread { get; } = new(new ThreadModel());

        public AssistantViewModel NewAssistantTemplateViewModel { get; }

        public ObservableCollection<AssistantViewModel> Assistants { get; } = new();

        public ObservableCollection<ToolViewModel> Toolset { get; } = new();

        public ObservableCollection<FileViewModel> Files { get; } = new();
        #endregion

        #region Constructors
        public ProjectViewModel(ProjectModel projectModel) : base(projectModel)
        {
            NewAssistantTemplateViewModel = new AssistantViewModel(_model.newAssistantTemplate)
            {
                IsTemplate = true,
                Modified = false
            };
            NewAssistantTemplateViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AssistantViewModel.Modified))
                {
                    Modified |= NewAssistantTemplateViewModel.Modified;
                }
            };
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            _solutionEvents = dte.Events.SolutionEvents;
            _solutionEvents.Opened += OnSolutionOpened;
            _projectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("ProjectItemsEvents");
            _projectItemsEvents.ItemAdded += OnItemAdded;
            _projectItemsEvents.ItemRemoved += OnItemRemoved;
            _projectItemsEvents.ItemRenamed += OnItemRenamed;

            _documentEvents = dte.Events.DocumentEvents;
            _documentEvents.DocumentSaved += OnDocumentSaved;

            // if solution is already opened, load it
            if (dte.Solution.IsOpen)
            {
                OnSolutionOpened();
            }
        }
        #endregion

        #region Methods
        private async void OnSolutionOpened()
        {
            await LoadSolutionAsync();
        }

        private async void OnItemRenamed(ProjectItem item, string oldName)
        {
            List<FileViewModel> files = new();
            GetAllFiles(files);
            var fileViewModel = files.FirstOrDefault(x => x.Path == oldName);
            if (fileViewModel == null)
            {
                return;
            }

            if (fileViewModel.Status == FileViewModel.FileStatus.Synced)
            {
                await fileViewModel.DeleteAsync();
            }

            Model.files.Remove(fileViewModel.Model);
            if (fileViewModel.Parent != null)
            {
                fileViewModel.Parent.Children.Remove(fileViewModel);
                var folderFileViewModel = fileViewModel.Parent;

                // remove empty folders iterating to root folder
                while (folderFileViewModel.Children.Count == 0)
                {
                    var parent = folderFileViewModel.Parent;
                    parent?.Children.Remove(folderFileViewModel);
                    folderFileViewModel = parent;
                    if (folderFileViewModel == null)
                    {
                        break;
                    }
                }
            }

            OnItemAdded(item);
        }

        private async void OnItemAdded(ProjectItem item)
        {
            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, DTEUtils.SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                var projectPath = Path.GetDirectoryName(item.ContainingProject.FullName)!;
                var relativePath = item.FileNames[0].Substring(projectPath.Length + 1);
                var folders = relativePath.Split(Path.DirectorySeparatorChar);
                var currentFolderPath = projectPath;
                var projectFileViewModel = Files.FirstOrDefault(x => x.Path == projectPath);
                if (projectFileViewModel == null)
                {
                    projectFileViewModel = new FileViewModel(new FileModel
                    {
                        name = item.ContainingProject.Name,
                        path = projectPath,
                        isFolder = true
                    });
                    Files.Add(projectFileViewModel);
                }

                var folderFileViewModel = projectFileViewModel;
                for (var index = 0; index < folders.Length - 1; index++)
                {
                    var folderName = folders[index];
                    currentFolderPath = Path.Combine(currentFolderPath, folderName);
                    folderFileViewModel =
                        projectFileViewModel.Children.FirstOrDefault(x => x.Path == currentFolderPath);
                    if (folderFileViewModel == null)
                    {
                        folderFileViewModel = new FileViewModel(new FileModel
                        {
                            name = folderName,
                            path = currentFolderPath,
                            isFolder = true
                        })
                        {
                            Parent = projectFileViewModel,
                            Status = FileViewModel.FileStatus.Synced
                        };
                        projectFileViewModel.Children.Add(folderFileViewModel);
                    }
                }

                var fileModel = Model.files.FirstOrDefault(x => x.path == item.FileNames[0]);
                if (fileModel == null)
                {
                    fileModel = new FileModel
                    {
                        name = item.Name,
                        path = item.FileNames[0]
                    };
                    Model.files.Add(fileModel);
                }

                var fileViewModel = new FileViewModel(fileModel)
                {
                    Parent = folderFileViewModel
                };
                var parentStatus = folderFileViewModel.Status;
                folderFileViewModel.Children.Add(fileViewModel);

                if (parentStatus == FileViewModel.FileStatus.Synced)
                {
                    await fileViewModel.SyncAsync();
                }

                Save();
            }
        }

        private async void OnItemRemoved(ProjectItem item)
        {
            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, DTEUtils.SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                var projectPath = Path.GetDirectoryName(item.ContainingProject.FullName)!;
                var projectFileViewModel = Files.FirstOrDefault(x => x.Path == projectPath);
                if (projectFileViewModel == null)
                {
                    return;
                }

                var relativePath = item.FileNames[0].Substring(projectPath.Length + 1);
                var folders = relativePath.Split(Path.DirectorySeparatorChar);
                var currentFolderPath = projectPath;
                var folderFileViewModel = projectFileViewModel;
                for (var index = 0; index < folders.Length - 1; index++)
                {
                    var folderName = folders[index];
                    currentFolderPath = Path.Combine(currentFolderPath, folderName);
                    folderFileViewModel =
                        projectFileViewModel.Children.FirstOrDefault(x => x.Path == currentFolderPath);
                    if (folderFileViewModel == null)
                    {
                        return;
                    }
                }

                var fileModel = Model.files.FirstOrDefault(x => x.path == item.FileNames[0]);
                if (fileModel == null)
                {
                    return;
                }

                var fileViewModel = folderFileViewModel.Children.FirstOrDefault(x => x.Path == item.FileNames[0]);
                if (fileViewModel == null)
                {
                    return;
                }

                if (fileViewModel.Status == FileViewModel.FileStatus.Synced)
                {
                    await DeleteFileAsync(fileViewModel);
                    Save();
                }

                folderFileViewModel.Children.Remove(fileViewModel);
                Model.files.Remove(fileModel);

                // remove empty folders iterating to root folder
                while (folderFileViewModel != projectFileViewModel && folderFileViewModel.Children.Count == 0)
                {
                    var parent = folderFileViewModel.Parent;
                    parent?.Children.Remove(folderFileViewModel);
                    folderFileViewModel = parent;
                }
            }
        }

        private async void OnDocumentSaved(Document document)
        {
            var item = document.ProjectItem;
            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, DTEUtils.SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                var projectPath = Path.GetDirectoryName(item.ContainingProject.FullName)!;
                var relativePath = item.FileNames[0].Substring(projectPath.Length + 1);
                var folders = relativePath.Split(Path.DirectorySeparatorChar);
                var currentFolderPath = projectPath;
                var projectFileViewModel = Files.FirstOrDefault(x => x.Path == projectPath);
                if (projectFileViewModel == null)
                {
                    return;
                }

                var folderFileViewModel = projectFileViewModel;
                for (var index = 0; index < folders.Length - 1; index++)
                {
                    var folderName = folders[index];
                    currentFolderPath = Path.Combine(currentFolderPath, folderName);
                    folderFileViewModel =
                        projectFileViewModel.Children.FirstOrDefault(x => x.Path == currentFolderPath);
                    if (folderFileViewModel == null)
                    {
                        return;
                    }
                }

                var fileModel = Model.files.FirstOrDefault(x => x.path == item.FileNames[0]);
                if (fileModel == null)
                {
                    return;
                }

                var fileViewModel = folderFileViewModel.Children.FirstOrDefault(x => x.Path == item.FileNames[0]);
                if (fileViewModel == null)
                {
                    return;
                }

                if (fileViewModel.Status == FileViewModel.FileStatus.Synced)
                {
                    await fileViewModel.DeleteAsync();
                    await fileViewModel.SyncAsync();
                    Save();
                }
            }
        }

        public void AddAssistant(AssistantModel assistant)
        {
            var assistantViewModel = new AssistantViewModel(assistant);
            Assistants.Add(assistantViewModel);
            assistantViewModel.RemoveAction += () => { Assistants.Remove(assistantViewModel); };
        }

        public void AddTool(ToolModel tool, bool loaded)
        {
            var toolViewModel = new ToolViewModel(tool)
            {
                Modified = !loaded
            };
            Toolset.Add(toolViewModel);
            toolViewModel.RemoveAction += () => { Toolset.Remove(toolViewModel); };
        }

        public async Task CreateToolAsync()
        {
            if (!ValidateSettings())
            {
                return;
            }

            if (Toolset.Count == 0)
            {
                if (!await LoadToolsetAsync())
                {
                    return;
                }
            }

            var toolName = "NewTool";
            var index = 1;
            while (Toolset.Any(t => t.Name == toolName))
                toolName = $"NewTool{index++}";
            var tool = new ToolModel
            {
                name = toolName,
                description = "Write tool description (used by GPT)",
                category = "Custom"
            };
            AddTool(tool, false);
        }

        public void Save()
        {
            try
            {
                ProjectUtils.SaveProject();
                NewAssistantTemplateViewModel.Modified = false;
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
                await LoadToolsetAsync();
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

                    AddAssistant(assistantModel);
                }
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                Working = false;
            }
        }

        public async Task<bool> LoadToolsetAsync()
        {
            if (!ValidateSettings())
            {
                return false;
            }

            try
            {
                Toolset.Clear();
                var toolsetJson = await ToolAPI.GetToolsetAsync();
                if (string.IsNullOrEmpty(toolsetJson))
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
                await OutputWindowHelper.LogErrorAsync(e);
                return false;
            }

            return true;
        }

        public async Task ReloadToolsetAsync()
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
                await OutputWindowHelper.LogErrorAsync(e);
                return;
            }

            await LoadToolsetAsync();
        }

        public async Task LoadSolutionAsync()
        {
            var filesToDelete = new List<FileModel>(Model.files);
            var solutionItems = DTEUtils.GetSolutionItems();
            foreach (var item in solutionItems)
            {
                var name = item.Name;
                var filePath = item.FileNames[0];
                var projectPath = Path.GetDirectoryName(item.ContainingProject.FullName)!;
                var projectFileViewModel = Files.FirstOrDefault(x => x.Path == projectPath);
                if (projectFileViewModel == null)
                {
                    projectFileViewModel = new FileViewModel(new FileModel
                    {
                        name = item.ContainingProject.Name,
                        path = projectPath,
                        isFolder = true
                    });
                    Files.Add(projectFileViewModel);
                }

                var relativePath = filePath.Substring(projectPath.Length + 1);
                var folders = relativePath.Split(Path.DirectorySeparatorChar);
                var currentFolderPath = projectPath;
                var folderFileViewModel = projectFileViewModel;
                for (var index = 0; index < folders.Length - 1; index++)
                {
                    var folderName = folders[index];
                    currentFolderPath = Path.Combine(currentFolderPath, folderName);
                    folderFileViewModel =
                        projectFileViewModel.Children.FirstOrDefault(x => x.Path == currentFolderPath);
                    if (folderFileViewModel == null)
                    {
                        folderFileViewModel = new FileViewModel(new FileModel
                        {
                            name = folderName,
                            path = currentFolderPath,
                            isFolder = true
                        })
                        {
                            Parent = projectFileViewModel
                        };
                        projectFileViewModel.Children.Add(folderFileViewModel);
                    }
                }

                var fileModel = Model.files.FirstOrDefault(x => x.path == filePath);
                if (fileModel == null)
                {
                    fileModel = new FileModel
                    {
                        name = name,
                        path = filePath
                    };
                    Model.files.Add(fileModel);
                    Modified = true;
                }
                else
                {
                    filesToDelete.Remove(fileModel);
                }

                var fileViewModel = new FileViewModel(fileModel)
                {
                    Parent = folderFileViewModel
                };
                folderFileViewModel.Children.Add(fileViewModel);
            }

            foreach (var fileToDelete in filesToDelete)
            {
                Model.files.Remove(fileToDelete);
                Modified = true;
            }

            var filesIdsToRemove = new List<string>();
            var filesSynced = new List<FileViewModel>();
            var listFilesRequest = new ListFilesRequest();
            var listFilesResponse = await listFilesRequest.SendAsync();
            JArray data = listFilesResponse.data;
            await OutputWindowHelper.LogInfoAsync(
                "Resources",
                $"{data.Count} files retrieved from the server...");
            var allFiles = new List<FileViewModel>();
            GetAllFiles(allFiles);
            foreach (var file in data)
            {
                var fileId = file["id"]!.ToString();
                var fileModel = allFiles.FirstOrDefault(x => x.Id == fileId);
                if (fileModel == null)
                {
                    filesIdsToRemove.Add(fileId);
                }
                else
                {
                    filesSynced.Add(fileModel);
                }
            }

            if (filesIdsToRemove.Count > 0)
            {
                await OutputWindowHelper.LogInfoAsync(
                    "Resources",
                    $"Removing {filesIdsToRemove.Count} unused files...");
            }

            foreach (var fileIdToRemove in filesIdsToRemove)
            {
                var request = new DeleteFileRequest(fileIdToRemove);
                await request.SendAsync();
            }

            foreach (var file in allFiles)
            {
                if (!string.IsNullOrEmpty(file.Id) && !filesSynced.Contains(file))
                {
                    file.Id = "";
                    Modified = true;
                }
            }

            if (Modified)
            {
                Save();
            }

            var listVectorStoresRequest = new ListVectorStoresRequest();
            var listVectorStoresResponse = await listVectorStoresRequest.SendAsync();
            JArray vectorStores = listVectorStoresResponse.data;
            await OutputWindowHelper.LogInfoAsync(
                "Resources",
                $"{vectorStores.Count} vector stores retrieved from the server...");
            // we just want to keep a single vector store, delete the rest
            for (var index = 1; index < vectorStores.Count; index++)
            {
                var vectorStoreId = vectorStores[index]["id"]!.ToString();
                await OutputWindowHelper.LogInfoAsync(
                    "Resources",
                    $"Removing vector store {vectorStoreId}...");
                var deleteVectorStoreRequest = new DeleteVectorStoreRequest(vectorStoreId);
                await deleteVectorStoreRequest.SendAsync();
            }

            // if there is no vector store, create one
            if (vectorStores.Count == 1)
            {
                var vectorStoreId = vectorStores[0]["id"]!.ToString();
                var vectorStoreViewModel = new VectorStoreViewModel(new VectorStoreModel
                {
                    id = vectorStoreId
                });
                VectorStoreViewModel = vectorStoreViewModel;
            }
            else
            {
                await OutputWindowHelper.LogInfoAsync(
                    "Resources",
                    "Creating a new vector store...");
                var createVectorStoreRequest = new CreateVectorStoreRequest();
                var createVectorStoreResponse = await createVectorStoreRequest.SendAsync();
                var vectorStoreId = createVectorStoreResponse.data["id"]!.ToString();
                var vectorStoreViewModel = new VectorStoreViewModel(new VectorStoreModel
                {
                    id = vectorStoreId
                });
                VectorStoreViewModel = vectorStoreViewModel;
            }

            await VectorStoreViewModel.SyncAsync();
        }

        private async Task DeleteFileAsync(FileViewModel file)
        {
            var fileModel = Model.files.FirstOrDefault(x => x.id == file.Id);
            if (fileModel != null)
            {
                Model.files.Remove(fileModel);
                Save();
            }

            await file.DeleteAsync();
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

        public void GetAllFiles(List<FileViewModel> files)
        {
            foreach (var file in Files)
            {
                GetAllFiles(files, file);
            }
        }

        private void GetAllFiles(List<FileViewModel> files, FileViewModel current)
        {
            if (!current.IsFolder)
            {
                files.Add(current);
            }

            foreach (var child in current.Children)
            {
                GetAllFiles(files, child);
            }
        }
        #endregion
    }
}