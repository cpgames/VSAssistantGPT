using System.Windows.Controls;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        #region Properties
        public MessageViewModel? ViewModel
        {
            get => DataContext as MessageViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public MessageControl()
        {
            InitializeComponent();
        }
        #endregion
    }
}