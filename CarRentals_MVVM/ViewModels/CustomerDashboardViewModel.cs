using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class CustomerDashboardViewModel : ObservableObject
    {
        private readonly string _userId;

        private bool _isSidebarOpen = false;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set { _isSidebarOpen = value; OnPropertyChanged(); OnPropertyChanged(nameof(SidebarWidth)); }
        }
        public GridLength SidebarWidth => IsSidebarOpen ? new GridLength(220) : new GridLength(0);

        public string UserLabel { get; }
        public string WelcomeText { get; }
        public string SubText { get; }

        public ICommand HamburgerCommand { get; }
        public ICommand BrowseCarsCommand { get; }
        public ICommand MyAccountCommand { get; }
        public ICommand MyRentalsCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand ChatCommand { get; }


        public CustomerDashboardViewModel(UserModel user)
        {
            _userId = user.UserID;

            // Use FullName from session if available, fallback to UserID
            string displayName = !string.IsNullOrEmpty(UserSession.FullName)
                ? UserSession.FullName : user.UserID;

            UserLabel = $"Customer: {UserSession.Username ?? user.UserID}";
            WelcomeText = $"Welcome back, {displayName}!";
            SubText = "Browse available cars or view your account.";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            BrowseCarsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            MyAccountCommand = new RelayCommand(_ =>
            NavigationService.Navigate(new View.MyAccountWindow(_userId)));

            MyRentalsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.MyRentalsWindow(_userId)));

            LogoutCommand = new RelayCommand(_ =>
            {
                var result = MessageBox.Show("Are you sure you want to log out?",
                    "Log Out", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    UserSession.Clear();
                    NavigationService.Navigate(new View.ChooseRole());
                }
            });

            // In CustomerDashboardViewModel — chat command
            ChatCommand = new RelayCommand(_ =>
                NavigationService.Navigate(
                    new View.ChatWindow(_userId, "A001", "Customer")));

        }
    }
}