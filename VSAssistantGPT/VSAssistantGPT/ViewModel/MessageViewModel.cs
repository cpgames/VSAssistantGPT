namespace cpGames.VSA.ViewModel
{
    public class MessageViewModel : ViewModel<MessageModel>
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

        public string Role
        {
            get => _model.role;
            set
            {
                if (_model.role != value)
                {
                    _model.role = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Text
        {
            get => _model.text;
            set
            {
                if (_model.text != value)
                {
                    _model.text = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public MessageViewModel(MessageModel model) : base(model) { }
        #endregion
    }
}