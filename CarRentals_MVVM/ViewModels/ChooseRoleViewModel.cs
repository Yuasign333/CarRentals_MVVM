using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for ChooseRole.xaml.
    /// Handles the role selection screen where the user picks Customer or Admin
    /// before proceeding to the login page.
    /// Connected to: ChooseRole.xaml (View), NavigationService (navigation),
    /// CustomerLogin.xaml and AdminLogin.xaml (next windows).
    /// </summary>
    public class ChooseRoleViewModel : ObservableObject
    {
        /// <summary>
        /// Navigates to the Customer login screen.
        /// Bound to the Customer card button in ChooseRole.xaml.
        /// </summary>
        public ICommand CustomerCommand { get; }

        /// <summary>
        /// Navigates to the Admin login screen.
        /// Bound to the Admin card button in ChooseRole.xaml.
        /// </summary>
        public ICommand AdminCommand { get; }

        /// <summary>
        /// Prompts the user to confirm exit, then shuts down the application.
        /// Bound to the Exit button in ChooseRole.xaml.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Initializes all commands for the role selection screen.
        /// </summary>
        public ChooseRoleViewModel()
        {
            // Navigate to CustomerLogin when the Customer card is clicked
            CustomerCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.CustomerLogin());
            });

            // Navigate to AdminLogin when the Admin card is clicked
            AdminCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AdminLogin());
            });

            // Ask for confirmation before closing the entire application
            ExitCommand = new RelayCommand(_ =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to Exit?",
                    "Exit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            });
        }
    }
}