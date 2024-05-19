using System;

namespace cpGames.VSA.ViewModel
{
    public class ToolEntryViewModel : ViewModel<ToolEntryModel>
    {
        #region Properties
        public Action? RemoveAction { get; set; }
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

        public string Category
        {
            get => _model.category;
            set
            {
                if (_model.category != value)
                {
                    _model.category = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public ToolEntryViewModel(ToolEntryModel model) : base(model) { }
        #endregion

        #region Methods
        public void Remove()
        {
            RemoveAction?.Invoke();
        }
        #endregion
    }
}