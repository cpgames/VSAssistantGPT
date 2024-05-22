using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class VectorStoreViewModel : ViewModel<VectorStoreModel>
    {
        #region Properties
        public Action? CreateAction { get; set; }
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
                }
            }
        }

        public ObservableCollection<FileViewModel> Files { get; } = new();
        #endregion

        #region Constructors
        public VectorStoreViewModel(VectorStoreModel model) : base(model) { }
        #endregion

        #region Methods
        public async Task CreateAsync()
        {
            if (ProjectUtils.ActiveProject == null)
            {
                throw new Exception("No active project.");
            }
            var request = new CreateVectorStoreRequest();
            var response = await request.SendAsync();
            Id = response.data["id"]!.ToString();
            CreateAction?.Invoke();
        }

        public async Task DeleteAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new Exception("Vector store is not loaded.");
            }
            var request = new DeleteVectorStoreRequest(Id);
            await request.SendAsync();
            RemoveAction?.Invoke();
        }

        public async Task LoadFilesAsync()
        {
            if (ProjectUtils.ActiveProject == null)
            {
                throw new Exception("No active project.");
            }
            if (string.IsNullOrEmpty(Id))
            {
                throw new Exception("Vector store is not loaded.");
            }
            if (ProjectUtils.ActiveProject.Files.Count == 0)
            {
                await ProjectUtils.ActiveProject.LoadFilesAsync();
            }
            Files.Clear();
            var request = new ListVectorStoreFilesRequest(Id);
            var response = await request.SendAsync();
            JArray data = response.data;
            foreach (var file in data)
            {
                var id = file["id"]!.ToString();
                var fileViewModel = ProjectUtils.ActiveProject.Files.FirstOrDefault(f => f.Id == id);
                if (fileViewModel == null)
                {
                    throw new Exception("File not found.");
                }
                Files.Add(fileViewModel);
                fileViewModel.RemoveAction += () =>
                {
                    Files.Remove(fileViewModel);
                };
            }
        }

        public async Task SyncFilesAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new Exception("Vector store is not loaded.");
            }
            if (ProjectUtils.ActiveProject == null)
            {
                throw new Exception("No active project.");
            }
            if (ProjectUtils.ActiveProject.Files.Count == 0)
            {
                await ProjectUtils.ActiveProject.LoadFilesAsync();
            }
            if (Files.Count == 0)
            {
                await LoadFilesAsync();
            }
            var selectedFiles = ProjectUtils.ActiveProject.Files.Where(f => f.Selected).ToList();
            var filesToDelete = new List<FileViewModel>();
            foreach (var fileViewModel in Files)
            {
                if (string.IsNullOrEmpty(fileViewModel.Id))
                {
                    continue;
                }
                if (!selectedFiles.Contains(fileViewModel))
                {
                    filesToDelete.Add(fileViewModel);
                }
            }
            foreach (var fileViewModel in filesToDelete)
            {
                var deleteVectorStoreFileRequest = new DeleteVectorStoreFileRequest(Id, fileViewModel.Id);
                await deleteVectorStoreFileRequest.SendAsync();
                Files.Remove(fileViewModel);
            }
            selectedFiles.RemoveAll(f => Files.Contains(f));
            if (selectedFiles.Count == 0)
            {
                return;
            }
            var createVectorStoreFileBatchRequest = new CreateVectorStoreFileBatchRequest(Id, selectedFiles.Select(f => f.Id).ToArray());
            await createVectorStoreFileBatchRequest.SendAsync();
            foreach (var fileViewModel in selectedFiles)
            {
                Files.Add(fileViewModel);
                fileViewModel.RemoveAction += () =>
                {
                    Files.Remove(fileViewModel);
                };
            }
        }
        #endregion
    }
}