using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class FileViewModel : ViewModel<FileModel>
    {
        #region FileStatus enum
        public enum FileStatus
        {
            NotSynced,
            Syncing,
            Deleting,
            Synced
        }
        #endregion

        #region Fields
        private FileStatus _status = FileStatus.NotSynced;
        #endregion

        #region Properties
        public string Id
        {
            get => _model.id;
            set
            {
                if (_model.id != value)
                {
                    _model.id = value;
                    OnPropertyChanged();
                    Status = string.IsNullOrEmpty(value) ? FileStatus.NotSynced : FileStatus.Synced;
                }
            }
        }

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

        public string Path
        {
            get => _model.path;
            set
            {
                if (_model.path != value)
                {
                    _model.path = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFolder
        {
            get => _model.isFolder;
            set
            {
                if (_model.isFolder != value)
                {
                    _model.isFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        public FileStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    if (Parent != null)
                    {
                        if (Parent.Children.All(x => x.Status == FileStatus.Synced))
                        {
                            Parent.Status = FileStatus.Synced;
                        }
                        else if (Parent.Children.Any(x => x.Status == FileStatus.Syncing))
                        {
                            Parent.Status = FileStatus.Syncing;
                        }
                        else if (Parent.Children.Any(x => x.Status == FileStatus.Deleting))
                        {
                            Parent.Status = FileStatus.Deleting;
                        }
                        else
                        {
                            Parent.Status = FileStatus.NotSynced;
                        }
                    }
                }
            }
        }

        public FileViewModel? Parent { get; set; }
        public ObservableCollection<FileViewModel> Children { get; } = new();
        #endregion

        #region Constructors
        public FileViewModel(FileModel model) : base(model)
        {
            if (string.IsNullOrEmpty(model.id))
            {
                Status = FileStatus.NotSynced;
            }
            else
            {
                Status = FileStatus.Synced;
            }
            Children.CollectionChanged += (sender, args) =>
            {
                Status = Children.All(x => x.Status == FileStatus.Synced) ? FileStatus.Synced : FileStatus.NotSynced;
            };
        }
        #endregion

        #region Methods
        public async Task DeleteAsync()
        {
            if (!IsFolder)
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return;
                }

                if (!ProjectUtils.ActiveProject.ValidateSettings())
                {
                    return;
                }

                try
                {
                    var request = new DeleteFileRequest(Id);
                    await request.SendAsync();
                    Id = string.Empty;
                }
                catch (Exception e)
                {
                    await OutputWindowHelper.LogErrorAsync(e);
                    ProjectUtils.ActiveProject.Working = false;
                }
            }
            else if (IsFolder)
            {
                foreach (var child in Children)
                {
                    await child.DeleteAsync();
                }
            }
        }

        public async Task SyncAsync()
        {
            if (Status != FileStatus.NotSynced)
            {
                return;
            }

            Status = FileStatus.Syncing;
            if (!IsFolder)
            {
                try
                {
                    var tmpDir = Utils.GetOrCreateAppDir("tmp");
                    string tmpFilePath;
                    if (!Path.StartsWith(tmpDir))
                    {
                        var content = File.ReadAllText(Path);

                        var fileObject = new JObject
                        {
                            ["document_path"] = Path,
                            ["data"] = content
                        };

                        tmpFilePath = System.IO.Path.Combine(tmpDir, System.IO.Path.GetFileName(Path));
                        File.WriteAllText(tmpFilePath, fileObject.ToString());
                    }
                    else
                    {
                        tmpFilePath = Path;
                    }

                    // Upload the file
                    var uploadFileRequest = new UploadFileRequest(tmpFilePath);
                    var uploadFileResponse = await uploadFileRequest.SendAsync();
                    Id = uploadFileResponse.id;

                    var createVectorStoreFileRequest = new CreateVectorStoreFileRequest(ProjectUtils.ActiveProject.VectorStoreViewModel!.Id, Id);
                    await createVectorStoreFileRequest.SendAsync();

                    // Delete the temporary file
                    File.Delete(tmpFilePath);

                    ProjectUtils.ActiveProject.Save();
                    Status = FileStatus.Synced;
                }
                catch (Exception e)
                {
                    await OutputWindowHelper.LogErrorAsync(e);
                    ProjectUtils.ActiveProject.Working = false;
                    Status = FileStatus.NotSynced;
                }
            }
            else if (IsFolder)
            {
                foreach (var child in Children)
                {
                    await child.SyncAsync();
                }
            }
        }
        #endregion
    }
}