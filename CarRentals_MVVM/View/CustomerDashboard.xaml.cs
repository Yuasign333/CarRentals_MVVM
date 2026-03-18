using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class CustomerDashboard : Window
    {
        private bool _sidebarOpen = false;

        public CustomerDashboard(string userId)
        {
            InitializeComponent();
           
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new CustomerDashboardViewModel(
                new Models.UserModel { UserID = userId, Role = "Customer" });
        }
        
        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            _sidebarOpen = !_sidebarOpen;
            var anim = new DoubleAnimation
            {
                To = _sidebarOpen ? 220 : 0,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(260)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            

            if (DataContext is CustomerDashboardViewModel vm)
                vm.HamburgerCommand.Execute(null);
        }

        private void BrowseCarsBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm) vm.BrowseCarsCommand.Execute(null);
        }
        private void RentCarBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm) vm.RentCarCommand.Execute(null);
        }
        private void MyRentalsBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm) vm.MyRentalsCommand.Execute(null);
        }
        private void LogoutBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm) vm.LogoutCommand.Execute(null);
        }
    }
}