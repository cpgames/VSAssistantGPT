using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using cpGames.VSA.RestAPI;

namespace cpGames.VSA.ViewModel
{
    public class AssistantViewModel : ViewModel<AssistantModel>
    {
        #region Fields
        private bool _modified;
        private bool _isTemplate;
        #endregion

        #region Properties
        public Action? CreateAction { get; set; }
        public Action? RemoveAction { get; set; }

        public bool Modified
        {
            get => _modified;
            set
            {
                if (_modified != value)
                {
                    _modified = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public string Name
        {
            get => _model.name;
            set
            {
                if (_model.name != value)
                {
                    _model.name = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public string GPTModel
        {
            get => _model.gptModel;
            set
            {
                if (_model.gptModel != value)
                {
                    _model.gptModel = value;
                    OnPropertyChanged();
                    Modified = true;
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
                    Modified = true;
                }
            }
        }

        public string Instructions
        {
            get => _model.instructions;
            set
            {
                if (_model.instructions != value)
                {
                    _model.instructions = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public string VectorStoreId
        {
            get => _model.vectorStoreId;
            set
            {
                if (_model.vectorStoreId != value)
                {
                    _model.vectorStoreId = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public bool IsTemplate
        {
            get => _isTemplate;
            set
            {
                if (_isTemplate != value)
                {
                    _isTemplate = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ToolEntryViewModel> Toolset { get; } = new();
        #endregion

        #region Constructors
        public AssistantViewModel(AssistantModel model) : base(model)
        {
            _modified = string.IsNullOrWhiteSpace(model.id);
            foreach (var tool in model.toolset)
            {
                var toolViewModel = new ToolEntryViewModel(tool);
                Toolset.Add(toolViewModel);
                toolViewModel.RemoveAction = () =>
                {
                    Toolset.Remove(toolViewModel);
                    Model.toolset.Remove(tool);
                    Modified = true;
                };
            }
        }
        #endregion

        #region Methods
        public async Task SaveAsync()
        {
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            if (ProjectUtils.ActiveProject.Toolset.Count == 0)
            {
                ProjectUtils.ActiveProject.LoadToolset();
            }
            try
            {
                var createAssistantRequest = new CreateOrModifyAssistantRequest(Model, ProjectUtils.ActiveProject.Toolset);
                var createAssistantResponse = await createAssistantRequest.SendAsync();
                Id = createAssistantResponse.id;
                Modified = false;
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                ProjectUtils.ActiveProject.Working = false;
            }
            CreateAction?.Invoke();
        }

        public async Task DeleteAsync()
        {
            if (!string.IsNullOrEmpty(Id))
            {
                if (!ProjectUtils.ActiveProject.ValidateSettings())
                {
                    return;
                }
                try
                {
                    var request = new DeleteAssistantRequest(Id);
                    await request.SendAsync();
                }
                catch (Exception e)
                {
                    await OutputWindowHelper.LogErrorAsync(e);
                    ProjectUtils.ActiveProject.Working = false;
                }
            }
            RemoveAction?.Invoke();
        }

        public void AddTool(ToolModel tool)
        {
            if (Toolset.Any(t => t.Name == tool.name))
            {
                return;
            }
            var toolEntry = new ToolEntryModel
            {
                name = tool.name,
                category = tool.category
            };
            var toolEntryViewModel = new ToolEntryViewModel(toolEntry);
            Toolset.Add(toolEntryViewModel);
            Model.toolset.Add(toolEntry);
            toolEntryViewModel.RemoveAction = () =>
            {
                Toolset.Remove(toolEntryViewModel);
                Model.toolset.Remove(toolEntry);
                Modified = true;
            };
            Modified = true;
        }
        #endregion
    }
}