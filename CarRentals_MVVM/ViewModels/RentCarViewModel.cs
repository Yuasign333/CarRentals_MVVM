using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for the Car Rental booking process.
    /// Handles user input validation for drivers and hours, calculates pricing (Base, Deposit, Total),
    /// and manages the transaction to save rentals and generate receipts.
    /// </summary>
    public class RentCarViewModel : ObservableObject
    {
        private readonly string _userId;

        /// <summary>Displays the logged-in user's name or ID in the header badge.</summary>
        public string UserLabel { get; }

        /// <summary>The specific car object being rented, passed from the browsing window.</summary>
        public CarModel SelectedCar { get; }

        private string _selectedColor = string.Empty;
        /// <summary>The color chosen by the user from the available options.</summary>
        public string SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); }
        }

        private string _driverName = string.Empty;
        /// <summary>
        /// Property for the Driver's Name with built-in validation.
        /// Prevents numbers, special characters, and duplicate active drivers.
        /// </summary>
        public string DriverName
        {
            get => _driverName;
            set
            {
                // Validation: Ensure only letters and spaces are used
                if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsDigit) || value.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)))
                {
                    MessageBox.Show("Please enter a valid driver name (letters and spaces only).", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Validation: Ensure this driver doesn't already have an active rental in the system
                else if (CarDataService.IsDriverNameInUse(value))
                {
                    MessageBox.Show("This driver name is already in use for another active rental. Please choose a different name.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    _driverName = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _hours = 1;
        /// <summary>
        /// Number of hours for the rental.
        /// Triggers a recalculation of all price-related properties on change.
        /// </summary>
        public int Hours
        {
            get => _hours;
            set
            {
                // Business Rule: Rental must be between 1 and 24 hours
                if (value >= 1 && value <= 24)
                {
                    _hours = value;
                    OnPropertyChanged();
                    // Notify UI that prices need to be updated
                    OnPropertyChanged(nameof(BasePrice));
                    OnPropertyChanged(nameof(Deposit));
                    OnPropertyChanged(nameof(TotalDue));
                }
                else
                {
                    MessageBox.Show("Please enter a valid number of hours (at least 1 to 24).", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private int _rentalDuration = 1;
        /// <summary>Alternative duration tracking field used for UI synchronization.</summary>
        public int RentalDuration
        {
            get => _rentalDuration;
            set
            {
                if (value > 24) _rentalDuration = 24;
                else if (value < 1) _rentalDuration = 1;
                else _rentalDuration = value;

                OnPropertyChanged(nameof(RentalDuration));
            }
        }

        /* ── COMPUTED PRICE PROPERTIES ────────────────────────────────────────── */

        /// <summary>Calculated as PricePerHour * Hours.</summary>
        public decimal BasePrice => SelectedCar != null ? SelectedCar.PricePerHour * _hours : 0;

        /// <summary>Calculated as 30% of the Base Price.</summary>
        public decimal Deposit => BasePrice * 0.3m;

        /// <summary>Total cost including the base price and the security deposit.</summary>
        public decimal TotalDue => BasePrice + Deposit;

        /* ── COMMANDS ──────────────────────────────────────────────────────────── */

        /// <summary>Navigates back to the Car Browser.</summary>
        public ICommand BackCommand { get; }

        /// <summary>Processes the booking: generates ID, saves to DB, and updates car status.</summary>
        public ICommand ConfirmCommand { get; }

        public RentCarViewModel(string userId, CarModel car)
        {
            _userId = userId;
            SelectedCar = car;

            // Set the label based on the global session or passed ID
            UserLabel = !string.IsNullOrEmpty(UserSession.Username)
                ? $"Customer: {UserSession.Username}"
                : $"Customer: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            ConfirmCommand = new AsyncRelayCommand(async _ =>
            {
                // Final check for empty fields before submission
                if (string.IsNullOrWhiteSpace(SelectedColor))
                {
                    MessageBox.Show("Please select a color.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(DriverName))
                {
                    MessageBox.Show("Please enter a driver name.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Hours <= 0)
                {
                    MessageBox.Show("Please enter valid rental hours.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // 1. Generate unique Rental ID (e.g., R0001)
                    string rentalId = await CarDataService.GetNextRentalId();

                    // 2. Map inputs to the Rental Model
                    var rental = new RentalModel
                    {
                        RentalId = rentalId,
                        CustomerId = !string.IsNullOrEmpty(UserSession.UserId) ? UserSession.UserId : _userId,
                        CarId = SelectedCar.CarId,
                        CarName = SelectedCar.Name,
                        DriverName = DriverName,
                        Color = SelectedColor,
                        Hours = Hours,
                        BasePrice = BasePrice,
                        Deposit = Deposit,
                        TotalAmount = TotalDue,
                        RentalDate = DateTime.Now,
                        Status = "Active"
                    };

                    // 3. Save transaction to Database
                    await CarDataService.SaveRental(rental);

                    // 4. Mark car as 'Rented' so it disappears from the browser
                    SelectedCar.Status = "Rented";
                    await CarDataService.UpdateCar(SelectedCar);

                    // 5. Generate the physical receipt file (TXT)
                    CarDataService.GenerateReceipt(rental);

                    MessageBox.Show(
                        $"Booking confirmed!\nRental ID: {rental.RentalId}\nTotal: ${TotalDue:F2}\n\nReceipt saved.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 6. Redirect to the User's personal account page
                    NavigationService.Navigate(new View.MyAccountWindow(_userId));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Booking failed: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}