using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class AdminChatListWindow : Window
    {
        public AdminChatListWindow(string adminId)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new AdminChatListViewModel(adminId);
        }
    }
}