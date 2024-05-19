using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace cpGames.VSA.ViewModel
{
    public abstract class ViewModel<TModel> : INotifyPropertyChanged
    {
        #region Fields
        protected readonly TModel _model;
        #endregion

        #region Properties
        public TModel Model => _model;
        #endregion

        #region Constructors
        protected ViewModel(TModel model)
        {
            _model = model;
        }
        #endregion

        #region Events
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
    }
}