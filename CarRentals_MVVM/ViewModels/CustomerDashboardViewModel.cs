// ─────────────────────────────────────────────────────────────────────────────
// FILE: CustomerDashboardViewModel.cs
// Connected to: CustomerDashboard.xaml (View), CustomerDashboard.xaml.cs
// Purpose: Manages the Customer Dashboard — sidebar open/close state,
//          welcome text, user label, and all navigation commands.
//          Uses UserSession (static) to pull the logged-in user's display name.
// Commands: All ICommand properties are bound in XAML — zero click handlers.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class CustomerDashboardViewModel : ObservableObject
    {
        // The logged-in customer's DB ID (e.g. "C001") — passed to all sub-windows
        private readonly string _userId;

        // ── Sidebar toggle ─────────────────────────────────────────────────────

        private bool _isSidebarOpen = false;

        /// <summary>
        /// Controls whether the animated sidebar is open or closed.
        /// Setting this fires OnPropertyChanged so the XAML animation triggers.
        /// SidebarWidth is also notified so the old GridLength binding still works
        /// if any window still uses it as a fallback.
        /// </summary>
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set
            {
                _isSidebarOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SidebarWidth)); // kept for backward compat
            }
        }

        /// <summary>
        /// Returns the sidebar's current width as a GridLength.
        /// 220 when open, 0 when closed.
        /// NOTE: The XAML now uses the AnimatedSidebarStyle (DoubleAnimation)
        /// instead of binding directly to this — kept here for compatibility.
        /// </summary>
        public GridLength SidebarWidth => IsSidebarOpen
            ? new GridLength(220)
            : new GridLength(0);

        // ── Display text ───────────────────────────────────────────────────────

        /// <summary>
        /// Shows "Customer: [username]" in the top-right badge.
        /// Falls back to UserID if session username is not set.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// Personalized greeting shown below the top bar.
        /// Uses FullName from UserSession if available; falls back to UserID.
        /// </summary>
        public string WelcomeText { get; }

        /// <summary>
        /// Subtitle shown under WelcomeText — static descriptive text.
        /// </summary>
        public string SubText { get; }

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>Toggles IsSidebarOpen — bound to the hamburger ☰ button.</summary>
        public ICommand HamburgerCommand { get; }

        /// <summary>Navigates to BrowseCarsWindow to view available cars.</summary>
        public ICommand BrowseCarsCommand { get; }

        /// <summary>Navigates to MyAccountWindow (profile + rental history hub).</summary>
        public ICommand MyAccountCommand { get; }

        /// <summary>Navigates to MyRentalsWindow — kept for sidebar compatibility.</summary>
        public ICommand MyRentalsCommand { get; }

        /// <summary>Asks for confirmation then logs out, clearing UserSession.</summary>
        public ICommand LogoutCommand { get; }

        /// <summary>Opens ChatWindow as a floating window (customer ↔ admin).</summary>
        public ICommand ChatCommand { get; }

        // ── Constructor ────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the Customer Dashboard ViewModel.
        /// Reads UserSession for display name — populated during login.
        /// </summary>
        /// <param name="user">
        /// UserModel passed from CustomerDashboard.xaml.cs containing the
        /// logged-in customer's UserID and Role.
        /// </param>
        public CustomerDashboardViewModel(UserModel user)
        {
            _userId = user.UserID;

            // Use FullName from session if set; fallback to raw UserID
            string displayName = !string.IsNullOrEmpty(UserSession.FullName)
                ? UserSession.FullName
                : user.UserID;

            // UserLabel shows username in top-right badge
            UserLabel = $"Customer: {UserSession.Username ?? user.UserID}";
            WelcomeText = $"Welcome back, {displayName}!";
            SubText = "Browse available cars or view your account.";

            // Toggle sidebar open/closed — no async needed, instant UI state change
            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            // Navigate to the car browsing screen
            BrowseCarsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            // Navigate to the My Account hub (profile + rental history)
            MyAccountCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.MyAccountWindow(_userId)));

            // Navigate to My Rentals (standalone rental history list)
            MyRentalsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.MyRentalsWindow(_userId)));

            // Logout — confirm first, then clear session and return to ChooseRole
            LogoutCommand = new RelayCommand(_ =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to log out?",
                    "Log Out",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Clear static session so no stale data leaks to next login
                    UserSession.Clear();
                    NavigationService.Navigate(new View.ChooseRole());
                }
            });

            // Open chat as a floating window — does NOT navigate away from dashboard
            // "A001" is the admin ID that the customer chats with
            ChatCommand = new RelayCommand(_ =>
                NavigationService.Navigate(
                    new View.ChatWindow(_userId, "A001", "Customer")));
        }
    }
}