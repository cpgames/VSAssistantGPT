using System;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class ToolViewModel : ViewModel<ToolModel>
    {
        #region Fields
        private bool _modified;
        private string _originalName = "";
        #endregion

        #region Properties
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
        public string Name
        {
            get => _model.name;
            set
            {
                if (_model.name != value)
                {
                    if (!Modified)
                    {
                        _originalName = _model.name;
                    }
                    _model.name = value;
                    OnPropertyChanged();
                    Modified = true;
                }
            }
        }

        public string Category
        {
            get => _model.category;
            set
            {
                if (_model.category != value)
                {
                    _model.category = value;
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
        #endregion

        #region Constructors
        public ToolViewModel(ToolModel model) : base(model) { }
        #endregion

        #region Methods
        public void Remove()
        {
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            try
            {
                ToolAPI.RemoveTool(Name);
            }
            catch (Exception e)
            {
                OutputWindowHelper.LogError("Error", e.Message);
            }
            RemoveAction?.Invoke();
        }

        public void Save()
        {
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            try
            {
                var toolData = new JObject
                {
                    ["name"] = Name,
                    ["category"] = Category,
                    ["description"] = Description
                };
                ToolAPI.SaveTool(_originalName, toolData);
            }
            catch (Exception e)
            {
                OutputWindowHelper.LogError("Error", e.Message);
            }
            Modified = false;
        }
        #endregion
    }
}