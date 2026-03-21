using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for MyRentalsWindow.xaml.
    /// Loads and displays all rental transactions that belong to the logged-in customer.
    /// Connected to: MyRentalsWindow.xaml (View),
    /// MyRentalsWindow.xaml.cs (sets DataContext to this ViewModel),
    /// CarDataService.GetByCustomer() (data source),
    /// BrowseCarsViewModel.ConfirmCommand (creates the rentals shown here).
    /// </summary>
    public class MyRentalsViewModel : ObservableObject
    {
        // The logged-in customer's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right of the window (e.g. "Customer: C001").
        /// Bound to the user badge TextBlock in MyRentalsWindow.xaml.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// The list of all rentals made by the current customer.
        /// Populated from CarDataService.GetByCustomer() on initialization.
        /// Bound to the rentals list in MyRentalsWindow.xaml.
        /// </summary>
        public ObservableCollection<RentalModel> Rentals { get; } = new();

        /// <summary>
        /// Returns true if the customer has at least one rental.
        /// Used to toggle the empty-state message vs. the rentals list in the XAML.
        /// </summary>
        public bool HasRentals => Rentals.Count > 0;

        /// <summary>
        /// Navigates back to the Customer Dashboard.
        /// Bound to the Back button in MyRentalsWindow.xaml.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the My Rentals view with the customer's rental history.
        /// </summary>
        /// <param name="userId">The logged-in customer's ID (e.g. "C001").</param>
        public MyRentalsViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Customer: {userId}";

            // Navigate back to the customer dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.CustomerDashboard(_userId));
            });

            // Load only the rentals that belong to this customer
            foreach (var rental in CarDataService.GetByCustomer(userId))
            {
                Rentals.Add(rental);
            }
        }
    }
}