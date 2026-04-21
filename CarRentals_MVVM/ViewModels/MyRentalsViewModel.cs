using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for MyRentalsWindow.xaml.
    /// Loads and displays all rental transactions that belong to the logged-in customer.
    /// Queries the database using the actual CustomerID from UserSession,
    /// not the username, to ensure the correct rentals are shown.
    /// Connected to: MyRentalsWindow.xaml (View),
    /// MyRentalsWindow.xaml.cs (sets DataContext),
    /// CarDataService.GetRentalsByCustomer() (data source),
    /// CustomerDashboard (navigates back here on Back).
    /// </summary>
    public class MyRentalsViewModel : ObservableObject
    {
        // The logged-in customer's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge.
        /// Displays the customer's username from UserSession if available,
        /// otherwise falls back to the raw userId.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// The list of all rentals made by the current customer.
        /// Loaded from CarDataService.GetRentalsByCustomer() on initialization.
        /// Bound to the rentals list in MyRentalsWindow.xaml.
        /// </summary>
        public ObservableCollection<RentalModel> Rentals { get; } = new();

        private bool _hasRentals = false;

        /// <summary>
        /// True when the customer has at least one rental.
        /// Must be a full property with OnPropertyChanged — NOT computed —
        /// because it is set after the async load completes and needs to trigger UI update.
        /// Used to toggle between the empty-state message and the rentals list.
        /// </summary>
        public bool HasRentals
        {
            get => _hasRentals;
            set
            {
                _hasRentals = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Navigates back to CustomerDashboard.
        /// Bound to the Back button in MyRentalsWindow.xaml.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the My Rentals ViewModel for the given customer.
        /// </summary>
        /// <param name="userId">The logged-in customer's user ID.</param>
        public MyRentalsViewModel(string userId)
        {
            _userId = userId;

            // Display username from session if available
            string displayName = !string.IsNullOrEmpty(UserSession.Username)
                ? UserSession.Username : userId;
            UserLabel = $"Customer: {displayName}";

            // Navigate back to the customer dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.CustomerDashboard(_userId));
            });

            // Load rentals from the database on a background thread
            Task.Run(async () =>
            {
                // Always query by the actual CustomerID (C001 etc), not the username
                // This ensures the correct rentals are shown even if username was changed
                string queryId = !string.IsNullOrEmpty(UserSession.UserId)
                    ? UserSession.UserId : userId;

                var rentals = await CarDataService.GetRentalsByCustomer(queryId);

                // Update the UI collection on the main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Rentals.Clear();
                    foreach (var r in rentals) Rentals.Add(r);

                    // Triggers OnPropertyChanged so the empty-state/list toggle updates
                    HasRentals = Rentals.Count > 0;
                });
            });
        }
    }
}