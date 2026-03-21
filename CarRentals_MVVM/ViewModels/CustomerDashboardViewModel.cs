using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for CustomerDashboard.xaml.
    /// Handles the customer's main hub — sidebar toggle, welcome text,
    /// and navigation to Browse Cars, Rent a Car, and My Rentals.
    /// Connected to: CustomerDashboard.xaml (View),
    /// BrowseCarsWindow (both Browse and Rent navigate here),
    /// MyRentalsWindow (navigation target),
    /// CustomerDashboard.xaml.cs (sets DataContext with this ViewModel).
    /// </summary>
    public class CustomerDashboardViewModel : ObservableObject
    {
        // The logged-in customer's user ID — used when navigating to sub-windows
        private readonly string _userId;

        // ── Sidebar toggle ─────────────────────────────────────────────────────

        private bool _isSidebarOpen = false;

        /// <summary>
        /// Whether the sidebar navigation panel is currently visible.
        /// Toggled by HamburgerCommand. Also notifies SidebarWidth to update.
        /// </summary>
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set
            {
                _isSidebarOpen = value;
                OnPropertyChanged();

                // Update the sidebar width binding when open state changes
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        /// <summary>
        /// Returns the sidebar's current width as a GridLength.
        /// 220 when open, 0 when closed.
        /// Bound to the sidebar column width in CustomerDashboard.xaml.
        /// </summary>
        public GridLength SidebarWidth
        {
            get
            {
                if (IsSidebarOpen)
                {
                    return new GridLength(220);
                }

                return new GridLength(0);
            }
        }

        // ── Display text ───────────────────────────────────────────────────────

        private string _userLabel = string.Empty;

        /// <summary>
        /// Label shown in the top-right of the dashboard (e.g. "Customer: C001").
        /// Bound to the user badge TextBlock in CustomerDashboard.xaml.
        /// </summary>
        public string UserLabel
        {
            get => _userLabel;
            set
            {
                _userLabel = value;
                OnPropertyChanged();
            }
        }

        private string _welcomeText = string.Empty;

        /// <summary>
        /// Personalized greeting shown in the dashboard body.
        /// Bound to the welcome TextBlock in CustomerDashboard.xaml.
        /// </summary>
        public string WelcomeText
        {
            get => _welcomeText;
            set
            {
                _welcomeText = value;
                OnPropertyChanged();
            }
        }

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>Toggles the sidebar open and closed.</summary>
        public ICommand HamburgerCommand { get; }

        /// <summary>
        /// Navigates to BrowseCarsWindow to view available cars.
        /// Both Browse Cars and Rent a Car lead to the same window —
        /// the customer selects a car there to begin the rental process.
        /// </summary>
        public ICommand BrowseCarsCommand { get; }

        /// <summary>
        /// Also navigates to BrowseCarsWindow (same as BrowseCarsCommand).
        /// Provided as a separate command for the "Rent a Car" dashboard card.
        /// </summary>
        public ICommand RentCarCommand { get; }

        /// <summary>Navigates to MyRentalsWindow to view rental history.</summary>
        public ICommand MyRentalsCommand { get; }

        /// <summary>Prompts for confirmation then logs out to ChooseRole.</summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Initializes the Customer Dashboard with user info and all navigation commands.
        /// </summary>
        /// <param name="user">
        /// The logged-in customer user passed in from CustomerDashboard.xaml.cs.
        /// </param>
        public CustomerDashboardViewModel(UserModel user)
        {
            // Store user info for navigation and display
            _userId = user.UserID;
            UserLabel = $"Customer: {user.UserID}";
            WelcomeText = $"Welcome back, {user.UserID}!";

            // Toggle sidebar visibility on hamburger button click
            HamburgerCommand = new RelayCommand(_ =>
            {
                IsSidebarOpen = !IsSidebarOpen;
            });

            // Both Browse and Rent navigate to BrowseCarsWindow
            // The customer selects a car there to start the rental flow
            BrowseCarsCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId));
            });

            RentCarCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId));
            });

            // Navigate to rental history
            MyRentalsCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.MyRentalsWindow(_userId));
            });

            // Logout — ask for confirmation before returning to role selection
            LogoutCommand = new RelayCommand(_ =>
            {
                var owner = Application.Current.MainWindow;

                var result = MessageBox.Show(
                    owner,
                    "Are you sure you want to log out?",
                    "Log Out",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    NavigationService.Navigate(new View.ChooseRole());
                }
            });
        }
    }
}