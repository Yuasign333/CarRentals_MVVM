using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.ViewModels;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.View
{
    public partial class ChatWindow : Window
    {
        // 1. ADD THIS: Empty constructor for the XAML Designer
        public ChatWindow()
        {
            InitializeComponent();
        }

        // 2. UPDATE THIS: Add ': this()' so it runs the empty one first
        public ChatWindow(string userId, string role) : this()
        {
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new ChatViewModel(userId, role);
        }

        private void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is ChatViewModel vm)
                vm.SendCommand.Execute(null);
        }
    }
}