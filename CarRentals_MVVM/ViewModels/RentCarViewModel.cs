using System;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class RentCarViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }

        // The car passed in from BrowseCarsWindow
        public CarModel SelectedCar { get; }

        // Color selection
        private string _selectedColor = string.Empty;
        public string SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); }
        }

        // Booking fields
        private string _driverName = string.Empty;
        public string DriverName
        {
            
            
            get => _driverName;
           
            set {
                // If user inputs number or special character, show error message and do not update the field
                if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsDigit) || value.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)))
                {
                    MessageBox.Show("Please enter a valid driver name (letters and spaces only).", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //else if same driver name is already used for another active rental, show error message and do not update the field
                else if (CarDataService.IsDriverNameInUse(value))
                {
                    MessageBox.Show("This driver name is already in use for another active rental. Please choose a different name.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    _driverName = value; OnPropertyChanged();

                }


            }
        }

        private int _hours = 1;
        public int Hours
        {
            get => _hours;
            set
            {
            
                // Use >= 1 so that exactly 1 to 24 hour is accepted
                if (value >= 1 && value <= 24)
                {
                    _hours = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BasePrice));
                    OnPropertyChanged(nameof(Deposit));
                    OnPropertyChanged(nameof(TotalDue));
                }
                else // Changed from a broken 'else if' to a standard 'else'
                {
                    MessageBox.Show("Please enter a valid number of hours (at least 1 to 24).", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private int _rentalDuration = 1; // Default value
        public int RentalDuration
        {
            get => _rentalDuration;
            set
            {
                // Enforce a limit (e.g., max 24 hours)
                if (value > 24)
                {
                    _rentalDuration = 24;
                    // Optional: Show a message or trigger a notification
                }
                else if (value < 1)
                {
                    _rentalDuration = 1; // Minimum 1 hour
                }
                else
                {
                    _rentalDuration = value;
                }

                OnPropertyChanged(nameof(RentalDuration));

      
            }
        }

        // Computed prices
        public decimal BasePrice => SelectedCar != null ? SelectedCar.PricePerHour * _hours : 0;
        public decimal Deposit => BasePrice * 0.3m;
        public decimal TotalDue => BasePrice + Deposit;

        public ICommand BackCommand { get; }
        public ICommand ConfirmCommand { get; }

        public RentCarViewModel(string userId, CarModel car)
        {
            _userId = userId;
            SelectedCar = car;
            UserLabel = $"Customer: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            ConfirmCommand = new RelayCommand(async _ =>
            {
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
                    string rentalId = await CarDataService.GetNextRentalId();

                    var rental = new RentalModel
                    {
                        RentalId = rentalId,
                        CustomerId = _userId,
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

                    await CarDataService.SaveRental(rental);

                    SelectedCar.Status = "Rented";
                    await CarDataService.UpdateCar(SelectedCar);

                    CarDataService.GenerateReceipt(rental);

                    MessageBox.Show(
                        $"Booking confirmed!\nRental ID: {rental.RentalId}\nTotal: ${TotalDue:F2}\n\nReceipt saved.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new View.MyRentalsWindow(_userId));
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