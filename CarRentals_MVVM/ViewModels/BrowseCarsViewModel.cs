using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for BrowseCarsWindow.xaml.
    /// Displays all available cars to the customer with category and fuel type filters.
    /// When a car is selected, navigates to RentCarWindow to begin the booking flow.
    /// Connected to: BrowseCarsWindow.xaml (View),
    /// BrowseCarsWindow.xaml.cs (sets DataContext),
    /// CarDataService.GetAll() (data source),
    /// RentCarWindow (navigates here on car selection),
    /// CustomerDashboard (navigates here on Back).
    /// </summary>
    public class BrowseCarsViewModel : ObservableObject
    {
        // The logged-in customer's user ID — passed through to RentCarWindow
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge.
        /// Displays the customer's username from UserSession if available,
        /// otherwise falls back to the raw userId.
        /// </summary>
        public string UserLabel { get; }

        // ── Filter properties ──────────────────────────────────────────────────

        private string _selectedCategory = "All";

        /// <summary>
        /// The selected category filter ("All", "Sedan", "SUV", "Van").
        /// Triggers ApplyFilter() automatically whenever the value changes.
        /// Bound to the Category ComboBox in BrowseCarsWindow.xaml.
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();

                // Re-filter the car list when the category changes
                ApplyFilter();
            }
        }

        private string _selectedFuel = "All";

        /// <summary>
        /// The selected fuel type filter ("All", "Standard Engine", "EV", "Hybrid Engine").
        /// Triggers ApplyFilter() automatically whenever the value changes.
        /// Bound to the Fuel Type ComboBox in BrowseCarsWindow.xaml.
        /// </summary>
        public string SelectedFuel
        {
            get => _selectedFuel;
            set
            {
                _selectedFuel = value;
                OnPropertyChanged();

                // Re-filter the car list when the fuel type changes
                ApplyFilter();
            }
        }

        /// <summary>
        /// The filtered list of available cars shown as cards.
        /// Only cars with Status = "Available" are included after filtering.
        /// Bound to the ItemsControl in BrowseCarsWindow.xaml.
        /// </summary>
        public ObservableCollection<CarModel> FilteredCars { get; } = new();

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>
        /// Navigates back to CustomerDashboard.
        /// Bound to the Back button in BrowseCarsWindow.xaml.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Called when the customer clicks a car card.
        /// If the car is Available: navigates to RentCarWindow passing the selected car.
        /// If the car is Rented or Maintenance: shows an info message.
        /// Receives the CarModel as CommandParameter.
        /// </summary>
        public ICommand SelectCarCommand { get; }

        /// <summary>
        /// Initializes the BrowseCars ViewModel for the given customer.
        /// </summary>
        /// <param name="userId">The logged-in customer's user ID.</param>
        public BrowseCarsViewModel(string userId)
        {
            _userId = userId;

            // Show username from session if available, otherwise show raw userId
            UserLabel = !string.IsNullOrEmpty(UserSession.Username)
                ? $"Customer: {UserSession.Username}"
                : $"Customer: {userId}";

            // Navigate back to the customer dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.CustomerDashboard(_userId));
            });

            // Handle car card click — only allow Available cars to proceed
            SelectCarCommand = new RelayCommand(car =>
            {
                if (car is CarModel selected && selected.Status == "Available")
                {
                    // Navigate to RentCarWindow with the selected car
                    NavigationService.Navigate(new View.RentCarWindow(_userId, selected));
                }
                else if (car is CarModel unavailable)
                {
                    // Inform the customer why this car cannot be rented
                    MessageBox.Show(
                        $"This car is currently {unavailable.Status} and cannot be rented.",
                        "Unavailable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            });

            // Load the initial car list on startup
            ApplyFilter();
        }

        /// <summary>
        /// Loads cars from CarDataService and filters by Status, Category, and Fuel Type.
        /// Always excludes Rented and Maintenance cars — customers only see Available ones.
        /// Called automatically when SelectedCategory or SelectedFuel changes,
        /// and once on initialization.
        /// </summary>
        private async void ApplyFilter()
        {
            FilteredCars.Clear();

            // Fetch all cars from the database
            var allCars = await CarDataService.GetAll();

            // Only show Available cars to the customer
            var query = allCars.Where(c => c.Status == "Available");

            // Apply category filter if not "All"
            if (SelectedCategory != "All")
            {
                query = query.Where(c => c.Category == SelectedCategory);
            }

            // Apply fuel type filter if not "All"
            if (SelectedFuel != "All")
            {
                query = query.Where(c => c.FuelType == SelectedFuel);
            }

            // Populate the observable collection for the UI
            foreach (var car in query)
            {
                FilteredCars.Add(car);
            }
        }
    }
}