using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for AdminDashboard.xaml.
    /// Handles the admin's main hub — sidebar toggle, welcome text,
    /// and navigation to all admin feature windows.
    /// Connected to: AdminDashboard.xaml (View),
    /// FleetStatusWindow, ProcessReturnWindow, AddCarWindow,
    /// MaintenanceWindow, RevenueWindow (navigation targets),
    /// AdminDashboard.xaml.cs (sets DataContext with this ViewModel).
    /// </summary>
    public class AdminDashboardViewModel : ObservableObject
    {
        // The logged-in admin's user ID — used when navigating to sub-windows
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
        /// Bound to the sidebar column width in AdminDashboard.xaml.
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
        /// Label shown in the top-right of the dashboard (e.g. "Agent: A001").
        /// Bound to the user badge TextBlock in AdminDashboard.xaml.
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
        /// Bound to the welcome TextBlock in AdminDashboard.xaml.
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

        /// <summary>Navigates to FleetStatusWindow.</summary>
        public ICommand FleetCommand { get; }

        /// <summary>Navigates to ProcessReturnWindow.</summary>
        public ICommand ReturnCommand { get; }

        /// <summary>Navigates to AddCarWindow.</summary>
        public ICommand AddCarCommand { get; }

        /// <summary>Navigates to MaintenanceWindow.</summary>
        public ICommand MaintenanceCommand { get; }

        /// <summary>Navigates to RevenueWindow.</summary>
        public ICommand RevenueCommand { get; }

        /// <summary>Prompts for confirmation then logs out to ChooseRole.</summary>
        public ICommand LogoutCommand { get; }


        /// <summary>
        /// Initializes the Admin Dashboard with user info and all navigation commands.
        /// </summary>
        /// <param name="user">
        /// The logged-in admin user passed in from AdminDashboard.xaml.cs.
        /// </param>
        public AdminDashboardViewModel(UserModel user)
        {
            // Store user info for navigation and display
            _userId = user.UserID;
            UserLabel = $"Agent: {user.UserID}";
            WelcomeText = $"Welcome back, {user.UserID}!";

            // Toggle sidebar visibility on hamburger button click
            HamburgerCommand = new RelayCommand(_ =>
            {
                IsSidebarOpen = !IsSidebarOpen;
            });

            // Navigate to each admin feature window, passing the userId through
            FleetCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.FleetStatusWindow(_userId));
            });

            ReturnCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.ProcessReturnWindow(_userId));
            });

            AddCarCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AddCarWindow(_userId));
            });

            MaintenanceCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.MaintenanceWindow(_userId));
            });

            RevenueCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.RevenueWindow(_userId));
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