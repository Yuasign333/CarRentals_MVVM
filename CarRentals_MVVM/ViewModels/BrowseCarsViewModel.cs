using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for BrowseCarsWindow.xaml.
    /// Manages a 3-page flow for the customer rental process:
    ///   Page 1 — Browse and filter available cars
    ///   Page 2 — Choose a color for the selected car
    ///   Page 3 — Fill in booking details and confirm the rental
    /// Connected to: BrowseCarsWindow.xaml (View),
    /// BrowseCarsWindow.xaml.cs (sets DataContext to this ViewModel),
    /// CarDataService (reads cars, writes rentals),
    /// MyRentalsWindow (navigates here after booking confirmed),
    /// CustomerDashboard (navigates here on Back).
    /// </summary>
    public class BrowseCarsViewModel : ObservableObject
    {
        // The logged-in customer's user ID — passed through all navigation calls
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right of the window (e.g. "Customer: C001").
        /// Bound to the user badge TextBlock in BrowseCarsWindow.xaml.
        /// </summary>
        public string UserLabel { get; }

        // ── Page switching ─────────────────────────────────────────────────────

        private int _page = 1;

        /// <summary>
        /// The current page number (1, 2, or 3).
        /// Changing this notifies IsPageList, IsPageColor, IsPageForm
        /// so the correct page section becomes visible in the XAML.
        /// </summary>
        public int Page
        {
            get => _page;
            set
            {
                _page = value;
                OnPropertyChanged();

                // Notify all page visibility flags when page changes
                OnPropertyChanged(nameof(IsPageList));
                OnPropertyChanged(nameof(IsPageColor));
                OnPropertyChanged(nameof(IsPageForm));
            }
        }

        /// <summary>
        /// True when Page 1 (car list) should be visible.
        /// Bound to the Visibility of the Page 1 ScrollViewer in BrowseCarsWindow.xaml.
        /// </summary>
        public bool IsPageList => Page == 1;

        /// <summary>
        /// True when Page 2 (color picker) should be visible.
        /// Bound to the Visibility of the Page 2 Border in BrowseCarsWindow.xaml.
        /// </summary>
        public bool IsPageColor => Page == 2;

        /// <summary>
        /// True when Page 3 (booking form) should be visible.
        /// Bound to the Visibility of the Page 3 ScrollViewer in BrowseCarsWindow.xaml.
        /// </summary>
        public bool IsPageForm => Page == 3;

        // ── Filter properties ──────────────────────────────────────────────────

        private string _selectedCategory = "All";

        /// <summary>
        /// The currently selected category filter ("All", "SUV", "Sedan", "Van").
        /// Triggers ApplyFilter() whenever it changes.
        /// Bound to the Category ComboBox on Page 1 of BrowseCarsWindow.xaml.
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();

                // Re-filter the car list whenever the category selection changes
                ApplyFilter();
            }
        }

        private string _selectedFuel = "All";

        /// <summary>
        /// The currently selected fuel type filter ("All", "Standard Engine", "EV").
        /// Triggers ApplyFilter() whenever it changes.
        /// Bound to the Fuel Type ComboBox on Page 1 of BrowseCarsWindow.xaml.
        /// </summary>
        public string SelectedFuel
        {
            get => _selectedFuel;
            set
            {
                _selectedFuel = value;
                OnPropertyChanged();

                // Re-filter the car list whenever the fuel type selection changes
                ApplyFilter();
            }
        }

        /// <summary>
        /// The filtered list of cars shown as cards on Page 1.
        /// Only includes cars with Status = "Available" after filtering.
        /// Bound to the ItemsControl in BrowseCarsWindow.xaml Page 1.
        /// </summary>
        public ObservableCollection<CarModel> FilteredCars { get; } = new();

        // ── Selected car ───────────────────────────────────────────────────────

        private CarModel? _selectedCar;

        /// <summary>
        /// The car the customer clicked on from the car list.
        /// Set by SelectCarCommand. Used on Page 2 and Page 3 to display car details.
        /// Notifies computed price properties when changed.
        /// </summary>
        public CarModel? SelectedCar
        {
            get => _selectedCar;
            set
            {
                _selectedCar = value;
                OnPropertyChanged();
            }
        }

        // ── Color picker ───────────────────────────────────────────────────────

        private string _selectedColor = string.Empty;

        /// <summary>
        /// The color chosen by the customer on Page 2.
        /// Must be selected before NextPageCommand allows proceeding to Page 3.
        /// Bound to the color ListBox SelectedItem in BrowseCarsWindow.xaml Page 2.
        /// </summary>
        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged();
            }
        }

        // ── Booking form fields ────────────────────────────────────────────────

        private string _driverName = string.Empty;

        /// <summary>
        /// The name of the driver entered on Page 3.
        /// Required — ConfirmCommand will show a warning if this is empty.
        /// Bound to the Driver Name TextBox in BrowseCarsWindow.xaml Page 3.
        /// </summary>
        public string DriverName
        {
            get => _driverName;
            set
            {
                _driverName = value;
                OnPropertyChanged();
            }
        }

        private int _hours = 1;

        /// <summary>
        /// The number of rental hours entered on Page 3.
        /// Required to be greater than 0. Used to compute BasePrice, Deposit, TotalDue.
        /// Bound to the Rental Duration TextBox in BrowseCarsWindow.xaml Page 3.
        /// </summary>
        public int Hours
        {
            get => _hours;
            set
            {
                _hours = value;
                OnPropertyChanged();

                // Recalculate all price fields when hours change
                OnPropertyChanged(nameof(BasePrice));
                OnPropertyChanged(nameof(Deposit));
                OnPropertyChanged(nameof(TotalDue));
            }
        }

        // ── Computed price properties ──────────────────────────────────────────

        /// <summary>
        /// The base rental cost = PricePerHour x Hours.
        /// Returns 0 if no car is selected.
        /// Bound to the Base Price row in the Rental Estimate box on Page 3.
        /// </summary>
        public decimal BasePrice
        {
            get
            {
                if (SelectedCar != null)
                {
                    return SelectedCar.PricePerHour * _hours;
                }

                return 0;
            }
        }

        /// <summary>
        /// The security deposit = 30% of BasePrice.
        /// Bound to the Deposit row in the Rental Estimate box on Page 3.
        /// </summary>
        public decimal Deposit => BasePrice * 0.3m;

        /// <summary>
        /// The total amount due = BasePrice + Deposit.
        /// Bound to the Total Due row in the Rental Estimate box on Page 3.
        /// Also saved to RentalModel.TotalAmount when booking is confirmed.
        /// </summary>
        public decimal TotalDue => BasePrice + Deposit;

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>Navigates back to CustomerDashboard.</summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Called when the customer clicks a car card on Page 1.
        /// If the car is Available: sets SelectedCar and moves to Page 2.
        /// If the car is Rented or Maintenance: shows an info message.
        /// </summary>
        public ICommand SelectCarCommand { get; }

        /// <summary>
        /// Moves from Page 2 to Page 3.
        /// Requires a color to be selected first — shows warning if not.
        /// </summary>
        public ICommand NextPageCommand { get; }

        /// <summary>
        /// Moves back one page (Page 3 to 2, or Page 2 to 1).
        /// </summary>
        public ICommand PrevPageCommand { get; }

        /// <summary>
        /// Validates booking fields and creates the rental transaction.
        /// Marks the car as Rented, saves to CarDataService, then navigates to MyRentals.
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// Initializes the Browse Cars ViewModel for the given customer.
        /// Loads the initial car list and sets up all commands.
        /// </summary>
        /// <param name="userId">The logged-in customer's ID.</param>
        public BrowseCarsViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Customer: {userId}";

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
                    // Set the selected car and move to the color picker page
                    SelectedCar = selected;
                    Page = 2;
                }
                else if (car is CarModel unavailable && unavailable.Status != "Available")
                {
                    // Inform the customer why this car cannot be rented
                    MessageBox.Show(
                        $"This car is currently {unavailable.Status} and cannot be rented.",
                        "Unavailable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            });

            // Move to booking form — only if a color was selected
            NextPageCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(SelectedColor))
                {
                    MessageBox.Show(
                        "Please select a color first.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (Page < 3)
                {
                    Page++;
                }
            });

            // Go back one page
            PrevPageCommand = new RelayCommand(_ =>
            {
                if (Page > 1)
                {
                    Page--;
                }
            });

            // Validate and confirm the booking
            ConfirmCommand = new RelayCommand(_ =>
            {
                // Driver name is required
                if (string.IsNullOrWhiteSpace(DriverName))
                {
                    MessageBox.Show(
                        "Please enter a driver name.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Rental hours must be a positive number
                if (Hours <= 0)
                {
                    MessageBox.Show(
                        "Please enter valid rental hours.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Build the rental record from the current booking data
                var rental = new RentalModel
                {
                    RentalId = $"R{CarDataService.Rentals.Count + 1:D4}",
                    CustomerId = _userId,
                    CarId = SelectedCar!.CarId,
                    CarName = SelectedCar.Name,
                    DriverName = DriverName,
                    Color = SelectedColor,
                    Hours = Hours,
                    TotalAmount = TotalDue,
                    RentalDate = DateTime.Now
                };

                // Mark the car as Rented so it no longer appears in the browse list
                SelectedCar.Status = "Rented";

                // Save the rental to the in-memory data store
                CarDataService.Rentals.Add(rental);

                // Confirm to the customer and navigate to their rental history
                MessageBox.Show(
                    $"Booking confirmed!\nRental ID: {rental.RentalId}\nTotal: ${TotalDue:F2}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                NavigationService.Navigate(new View.MyRentalsWindow(_userId));
            });

            // Load the initial car list on startup
            ApplyFilter();
        }

        /// <summary>
        /// Filters the car list based on the selected Category and Fuel Type.
        /// Always excludes Rented and Maintenance cars — customers only see Available ones.
        /// Called automatically whenever SelectedCategory or SelectedFuel changes.
        /// </summary>
        private void ApplyFilter()
        {
            FilteredCars.Clear();

            var query = CarDataService.GetAll().AsEnumerable();

            // Customers should only see Available cars — Rented and Maintenance are hidden
            query = query.Where(c => c.Status == "Available");

            // Apply category filter if a specific category is selected
            if (SelectedCategory != "All")
            {
                query = query.Where(c => c.Category == SelectedCategory);
            }

            // Apply fuel type filter if a specific fuel type is selected
            if (SelectedFuel != "All")
            {
                query = query.Where(c => c.FuelType == SelectedFuel);
            }

            // Add the filtered results to the observable collection
            foreach (var car in query)
            {
                FilteredCars.Add(car);
            }
        }
    }
}