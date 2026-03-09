using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard(string userId)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new AdminDashboardViewModel(
                new Models.UserModel { UserID = userId, Role = "Admin" });
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.HamburgerCommand.Execute(null);
        }
        private void FleetBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.FleetCommand.Execute(null);
        }
        private void ReturnBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.ReturnCommand.Execute(null);
        }
        private void AddCarBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.AddCarCommand.Execute(null);
        }
        private void MaintenanceBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.MaintenanceCommand.Execute(null);
        }
        private void RevenueBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.RevenueCommand.Execute(null);
        }
        private void LogoutBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
                vm.LogoutCommand.Execute(null);
        }
    }
}