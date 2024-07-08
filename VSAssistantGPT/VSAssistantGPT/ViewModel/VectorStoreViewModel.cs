using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class VectorStoreViewModel : ViewModel<VectorStoreModel>
    {
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
                }
            }
        }
        #endregion

        #region Constructors
        public VectorStoreViewModel(VectorStoreModel model) : base(model) { }
        #endregion

        #region Methods
        public async Task SyncAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                await OutputWindowHelper.LogErrorAsync("Vector store is not loaded.");
                return;
            }

            var request = new ListVectorStoreFilesRequest(Id);
            var response = await request.SendAsync();
            JArray data = response.data;
            await OutputWindowHelper.LogInfoAsync("Resources", $"{data.Count} files found in vector store");
            try
            {
                var files = ProjectUtils.ActiveProject.GetAllFiles()
                    .Where(f => f.Status == FileViewModel.FileStatus.Synced && !f.IsFolder).ToList();
                var fileIdsToDelete = new List<string>();
                foreach (var file in data)
                {
                    if (files.All(x => x.Id != file["id"]!.ToString()))
                    {
                        fileIdsToDelete.Add(file["id"]!.ToString());
                    }
                    else
                    {
                        files.RemoveAll(x => x.Id == file["id"]!.ToString());
                    }
                }

                foreach (var fileId in fileIdsToDelete)
                {
                    var deleteVectorStoreFileRequest = new DeleteVectorStoreFileRequest(Id, fileId);
                    await deleteVectorStoreFileRequest.SendAsync();
                }

                if (files.Count > 0)
                {
                    var createVectorStoreFileBatchRequest =
                        new CreateVectorStoreFileBatchRequest(Id, files.Select(f => f.Id).ToArray());
                    await createVectorStoreFileBatchRequest.SendAsync();
                }
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                ProjectUtils.ActiveProject.Working = false;
            }
        }
        #endregion
    }
}