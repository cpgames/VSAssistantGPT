using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class FileViewModel : ViewModel<FileModel>
    {
        #region Fields
        private bool _selected;
        private Visibility _uploadedVisibility = Visibility.Visible;
        #endregion

        #region Properties
        public Action? RemoveAction { get; set; }
        public string Id
        {
            get => _model.id;
            set
            {
                if (_model.id != value)
                {
                    _model.id = value;
                    OnPropertyChanged();
                    UploadedVisibility = string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
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

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility UploadedVisibility
        {
            get => _uploadedVisibility;
            set
            {
                if (_uploadedVisibility != value)
                {
                    _uploadedVisibility = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public FileViewModel(FileModel model) : base(model)
        {
            UploadedVisibility = string.IsNullOrEmpty(Id) ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region Methods
        public async Task DeleteAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                await OutputWindowHelper.LogErrorAsync("Error", "File is not uploaded yet.");
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
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync("Error", e.Message);
            }
            RemoveAction?.Invoke();
        }

        public async Task UploadAsync()
        {
            if (!string.IsNullOrEmpty(Id))
            {
                await OutputWindowHelper.LogErrorAsync("Error", "File is already uploaded.");
                return;
            }
            try
            {
                var tmpDir = Utils.GetOrCreateAppDir("tmp");
                string tmpFilePath;
                if (!Path.StartsWith(tmpDir))
                {
                    var content = File.ReadAllText(Path);
                    var relativePath = DTEUtils.GetRelativePath(Path);

                    var fileObject = new JObject
                    {
                        ["path"] = relativePath,
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
                var request = new UploadFileRequest(tmpFilePath);
                var response = await request.SendAsync();
                Id = response.id;

                // Delete the temporary file
                File.Delete(tmpFilePath);
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync("Error", e.Message);
            }
        }
        #endregion
    }
}