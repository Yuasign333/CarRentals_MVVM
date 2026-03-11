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
            set
            {
                _isSidebarOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        public GridLength SidebarWidth
            => IsSidebarOpen ? new GridLength(220) : new GridLength(0);

        private string _userLabel = string.Empty;
        public string UserLabel
        {
            get => _userLabel;
            set { _userLabel = value; OnPropertyChanged(); }
        }

        private string _welcomeText = string.Empty;
        public string WelcomeText
        {
            get => _welcomeText;
            set { _welcomeText = value; OnPropertyChanged(); }
        }

        public ICommand HamburgerCommand { get; }
        public ICommand BrowseCarsCommand { get; }
        public ICommand RentCarCommand { get; }
        public ICommand MyRentalsCommand { get; }
        public ICommand LogoutCommand { get; }

        public CustomerDashboardViewModel(UserModel user)
        {
            _userId = user.UserID;
            UserLabel = $"Customer: {user.UserID}";
            WelcomeText = $"Welcome back, {user.UserID}!";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            // ✅ Both Browse and Rent go to BrowseCarsWindow — no ghost RentCarWindow
            BrowseCarsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            RentCarCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            MyRentalsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.MyRentalsWindow(_userId)));

            LogoutCommand = new RelayCommand(_ =>
            {
                var owner = Application.Current.MainWindow;
                var result = MessageBox.Show(owner,
                    "Are you sure you want to log out?", "Log Out",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    NavigationService.Navigate(new View.ChooseRole());
            });
        }
    }
}