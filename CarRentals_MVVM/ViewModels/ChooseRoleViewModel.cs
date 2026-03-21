using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class ChooseRoleViewModel : ObservableObject
    {
        public ICommand CustomerCommand { get; }
        public ICommand AdminCommand { get; }
        public ICommand ExitCommand { get; }

        public ChooseRoleViewModel()
        {
            CustomerCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerLogin()));

            AdminCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminLogin()));

            ExitCommand = new RelayCommand(_ =>
            {
                if (MessageBox.Show("Are you sure you want to Exit?", "Exit",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            });
        }
    }
}