using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class CustomerDashboard : Window
    {
        public CustomerDashboard(string userId)
        {
            InitializeComponent();

            // SetCurrent after window is fully ready
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new CustomerDashboardViewModel(
                new Models.UserModel { UserID = userId, Role = "Customer" });
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm)
                vm.HamburgerCommand.Execute(null);
        }

        private void BrowseCarsBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm)
                vm.BrowseCarsCommand.Execute(null);
        }

        private void RentCarBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm)
                vm.RentCarCommand.Execute(null);
        }

        private void MyRentalsBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm)
                vm.MyRentalsCommand.Execute(null);
        }

        private void LogoutBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerDashboardViewModel vm)
                vm.LogoutCommand.Execute(null);
        }
    }
}