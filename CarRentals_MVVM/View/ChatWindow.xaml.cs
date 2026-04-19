using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class ChatWindow : Window
    {
        public ChatWindow(string myId, string otherId, string role)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new ChatViewModel(myId, otherId, role);
        }

        private void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is ChatViewModel vm)
                {
                    vm.SendCommand.Execute(null);
                }
            }
        }
    }
}