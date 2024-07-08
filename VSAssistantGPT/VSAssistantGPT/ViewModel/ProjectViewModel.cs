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
        private DocumentEvents _documentEvents;
        private List<Document> _documentsSaved = new();
        private readonly List<ProjectItem> _itemsAdded = new();
        private List<ProjectItem> _itemsRemoved = new();
        private readonly List<FileRenamed> _itemsRenamed = new();
        private bool _modified;
        private ProjectItemsEvents _projectItemsEvents;
        private SolutionEvents _solutionEvents;
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

        public AssistantViewModel NewAssistantTemplateViewModel { get; private set; }

        public ObservableCollection<AssistantViewModel> Assistants { get; } = new();

        public ObservableCollection<ToolViewModel> Toolset { get; } = new();

        public ObservableCollection<FileViewModel> Files { get; } = new();
        #endregion

        #region Constructors
        public ProjectViewModel(ProjectModel projectModel) : base(projectModel) { }
        #endregion

        #region Methods
        public void Load()
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
            _projectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("ProjectItemsEvents");
            _projectItemsEvents.ItemAdded += OnItemAdded;
            _projectItemsEvents.ItemRemoved += OnItemRemoved;
            _projectItemsEvents.ItemRenamed += OnItemRenamed;
            _solutionEvents.Opened += OnSolutionOpened;
            _solutionEvents.AfterClosing += OnSolutionClosed;

            _documentEvents = dte.Events.DocumentEvents;
            _documentEvents.DocumentSaved += OnDocumentSaved;

            // listen to the PropertyChanged event Thread IsRunning property
            Thread.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ThreadViewModel.IsRunning))
                {
                    if (!Thread.IsRunning)
                    {
                        ThreadFinished();
                    }
                }
            };
        }

        private async void ThreadFinished()
        {

        }

        private async void OnSolutionOpened()
        {
            await LoadSolutionAsync();
        }

        private void OnSolutionClosed()
        {
            Files.Clear();
            Model.fileCache.Clear();
            Modified = false;
        }

        private async void OnItemRenamed(ProjectItem item, string oldName)
        {
            if (Thread.IsRunning)
            {
                _itemsRenamed.Add(new FileRenamed
                {
                    item = item,
                    oldName = oldName
                });
                return;
            }

            var file = GetAllFiles().FirstOrDefault(x => x.Path == oldName);
            var fileSynced = false;
            if (file != null)
            {
                fileSynced = file.Status == FileViewModel.FileStatus.Synced;
                if (fileSynced)
                {
                    await file.DeleteAsync();
                }

                Model.fileCache.Remove(file.Model);
                Save();

                var current = file;
                while (current.Parent != null)
                {
                    current.Parent.Children.Remove(current);

                    if (current.Parent.Children.Count > 0)
                    {
                        break;
                    }

                    current = current.Parent;
                }

                if (current.Parent == null && current.Children.Count == 0)
                {
                    Files.Remove(current);
                }
            }

            OnItemAdded(item);

            file = GetAllFiles().FirstOrDefault(x => x.Path == item.FileNames[0]);
            if (file != null && fileSynced)
            {
                await file.SyncAsync();
            }
        }

        private async void OnItemAdded(ProjectItem item)
        {
            if (Thread.IsRunning)
            {
                _itemsAdded.Add(item);
                return;
            }

            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, DTEUtils.SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                var files = GetAllFiles();
                var folderPath = Path.GetDirectoryName(item.FileNames[0])!;
                var relativePath = string.Empty;
                FileViewModel? parent = null;
                while (folderPath.Length > 0)
                {
                    parent = files.FirstOrDefault(x => x.Path == folderPath);
                    if (parent != null)
                    {
                        break;
                    }

                    // remove last folder from folderPath and add it to relativePath
                    var index = folderPath.LastIndexOf(Path.DirectorySeparatorChar);
                    if (index == -1)
                    {
                        break;
                    }

                    relativePath = folderPath.Substring(index + 1) + Path.DirectorySeparatorChar + relativePath;
                    folderPath = folderPath.Substring(0, index);
                }

                var file = new FileViewModel(new FileModel
                {
                    name = item.Name,
                    path = item.FileNames[0]
                });
                Model.fileCache.Add(file.Model);

                if (parent == null)
                {
                    Files.Add(file);
                    await file.SyncAsync();
                }
                else
                {
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        var folders = relativePath.Split(new[] { Path.DirectorySeparatorChar },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var folder in folders)
                        {
                            var newParent = new FileViewModel(new FileModel
                            {
                                name = folder,
                                path = parent.Path + Path.DirectorySeparatorChar + folder,
                                isFolder = true
                            })
                            {
                                Status = parent.Status
                            };
                            parent.Children.Add(newParent);
                            newParent.Parent = parent;
                            parent = newParent;
                        }
                    }

                    var parentSynced = parent.Status == FileViewModel.FileStatus.Synced;
                    parent.Children.Add(file);
                    file.Parent = parent;
                    if (parentSynced)
                    {
                        await file.SyncAsync();
                    }
                }

                Save();
            }
        }

        private async void OnItemRemoved(ProjectItem item)
        {
            if (Thread.IsRunning)
            {
                _itemsAdded.RemoveAll(x => x.FileNames[0] == item.FileNames[0]);
                _itemsRenamed.RemoveAll(x => x.item.FileNames[0] == item.FileNames[0]);
                _documentsSaved.RemoveAll(x => x.Path == item.FileNames[0]);
                _itemsRemoved.Add(item);
                return;
            }
            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, DTEUtils.SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                var fileName = item.FileNames[0]!;
                var files = GetAllFiles();
                var file = files.FirstOrDefault(x => x.Path == fileName);
                if (file == null)
                {
                    return;
                }

                if (file.Status == FileViewModel.FileStatus.Synced)
                {
                    await file.DeleteAsync();
                }

                Model.fileCache.Remove(file.Model);
                Save();

                var current = file;
                while (current.Parent != null)
                {
                    current.Parent.Children.Remove(current);

                    if (current.Parent.Children.Count > 0)
                    {
                        break;
                    }

                    current = current.Parent;
                }

                if (current.Parent == null && current.Children.Count == 0)
                {
                    Files.Remove(current);
                }
            }
        }

        private async void OnDocumentSaved(Document document)
        {
            if (Thread.IsRunning)
            {
                if (!_documentsSaved.Contains(document))
                {
                    _documentsSaved.Add(document);
                }
                _documentsSaved.Add(document);
                return;
            }
            var item = document.ProjectItem;
            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, DTEUtils.SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                var fileName = item.FileNames[0]!;
                var files = GetAllFiles();
                var file = files.FirstOrDefault(x => x.Path == fileName);
                if (file == null)
                {
                    return;
                }

                if (file.Status == FileViewModel.FileStatus.Synced)
                {
                    await file.DeleteAsync();
                    await file.SyncAsync();
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
            try
            {
                Files.Clear();
                if (!ValidateSettings())
                {
                    return;
                }

                var filesToDelete = new List<FileModel>(Model.fileCache);
                var solutionItems = DTEUtils.GetSolutionItems();
                foreach (var item in solutionItems)
                {
                    var name = item.Name;
                    var filePath = item.FileNames[0]!;
                    var folders = filePath.Split(Path.DirectorySeparatorChar);
                    var currentFolderPath = string.Empty;
                    FileViewModel? folderFileViewModel = null;
                    for (var index = 0; index < folders.Length - 1; index++)
                    {
                        var folderName = folders[index];
                        currentFolderPath = string.IsNullOrEmpty(currentFolderPath)
                            ? folderName
                            : currentFolderPath + Path.DirectorySeparatorChar + folderName;
                        var newFolderFileViewModel = folderFileViewModel != null
                            ? folderFileViewModel.Children.FirstOrDefault(x => x.Path == currentFolderPath)
                            : Files.FirstOrDefault(x => x.Path == currentFolderPath);
                        if (newFolderFileViewModel == null)
                        {
                            newFolderFileViewModel = new FileViewModel(new FileModel
                            {
                                name = folderName,
                                path = currentFolderPath,
                                isFolder = true
                            })
                            {
                                Parent = folderFileViewModel
                            };
                            if (folderFileViewModel == null)
                            {
                                Files.Add(newFolderFileViewModel);
                            }
                            else
                            {
                                folderFileViewModel.Children.Add(newFolderFileViewModel);
                            }
                        }

                        folderFileViewModel = newFolderFileViewModel;
                    }

                    var fileModel = Model.fileCache.FirstOrDefault(x => x.path == filePath);
                    if (fileModel == null)
                    {
                        fileModel = new FileModel
                        {
                            name = name,
                            path = filePath
                        };
                        Model.fileCache.Add(fileModel);
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
                    if (folderFileViewModel == null)
                    {
                        Files.Add(fileViewModel);
                    }
                    else
                    {
                        folderFileViewModel.Children.Add(fileViewModel);
                    }
                }

                TrimParentFolders();

                foreach (var fileToDelete in filesToDelete)
                {
                    Model.fileCache.Remove(fileToDelete);
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
                var files = GetAllFiles();
                foreach (var file in data)
                {
                    var fileId = file["id"]!.ToString();
                    var fileModel = files.FirstOrDefault(x => x.Id == fileId);
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

                foreach (var file in files)
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
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                throw;
            }
        }

        private void TrimParentFolders()
        {
            while (Files.Count == 1)
            {
                var file = Files[0];
                if (!file.IsFolder)
                {
                    break;
                }

                Files.Clear();
                foreach (var child in file.Children)
                {
                    child.Parent = null;
                    Files.Add(child);
                }

                file.Children.Clear();
            }
        }

        private async Task DeleteFileAsync(FileViewModel file)
        {
            var fileModel = Model.fileCache.FirstOrDefault(x => x.id == file.Id);
            if (fileModel != null)
            {
                Model.fileCache.Remove(fileModel);
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

        public List<FileViewModel> GetAllFiles()
        {
            var files = new List<FileViewModel>();
            foreach (var file in Files)
            {
                GetAllFiles(files, file);
            }

            return files;
        }

        private void GetAllFiles(List<FileViewModel> files, FileViewModel current)
        {
            files.Add(current);

            foreach (var child in current.Children)
            {
                GetAllFiles(files, child);
            }
        }

        public string GetTreePath(string documentPath)
        {
            var files = GetAllFiles();
            var file = files.FirstOrDefault(x => x.Path == documentPath);
            if (file == null)
            {
                return string.Empty;
            }

            var treePath = file.Name;
            while (file.Parent != null)
            {
                file = file.Parent;
                treePath = file.Name + Path.DirectorySeparatorChar + treePath;
            }

            return treePath;
        }
        #endregion

        #region Nested type: FileRenamed
        private struct FileRenamed
        {
            public ProjectItem item;
            public string oldName;
        }
        #endregion
    }
}