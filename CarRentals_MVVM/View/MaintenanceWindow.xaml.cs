using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class MaintenanceWindow : Window
    {
        public MaintenanceWindow(string userId)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new StubWindowViewModel(userId, isAdmin: true);
        }

        private void BackBtn_Click(object sender, MouseButtonEventArgs e)
        { if (DataContext is StubWindowViewModel vm) vm.BackCommand.Execute(null); }
    }
}