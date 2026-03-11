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
        public CarModel Car { get; }

        // ── Steps ──────────────────────────────────────────────
        private int _step = 1;
        public int Step
        {
            get => _step;
            set
            {
                _step = value; OnPropertyChanged();
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
            }
        }
        public bool IsStep1 => Step == 1;
        public bool IsStep2 => Step == 2;

        // ── Step 1 – Color ─────────────────────────────────────
        private string _selectedColor = string.Empty;
        public string SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); }
        }

        // ── Step 2 – Booking details ───────────────────────────
        private string _driverName = string.Empty;
        public string DriverName
        {
            get => _driverName;
            set { _driverName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConfirm)); }
        }

        private int _hours = 1;
        public int Hours
        {
            get => _hours;
            set
            {
                _hours = value < 1 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BasePrice));
                OnPropertyChanged(nameof(TotalDue));
            }
        }

        public decimal Deposit => 50m;
        public decimal BasePrice => Hours * Car.PricePerHour;
        public decimal TotalDue => BasePrice + Deposit;

        public bool CanConfirm => !string.IsNullOrWhiteSpace(DriverName);

        // ── Commands ───────────────────────────────────────────
        public ICommand SelectColorCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand BackStepCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand BackCommand { get; }

        public RentCarViewModel(string userId, CarModel car)
        {
            _userId = userId;
            Car = car;
            UserLabel = $"Customer: {userId}";
            SelectedColor = car.AvailableColors.Length > 0 ? car.AvailableColors[0] : "White";

            SelectColorCommand = new RelayCommand(c => { if (c is string col) SelectedColor = col; });
            NextCommand = new RelayCommand(_ => { if (Step < 2) Step++; });
            BackStepCommand = new RelayCommand(_ => { if (Step > 1) Step--; });
            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            ConfirmCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(DriverName))
                {
                    MessageBox.Show("Please enter a driver name.", "Required",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var rental = new RentalModel
                {
                    RentalId = $"R{CarDataService.Rentals.Count + 1:D4}",
                    CustomerId = _userId,
                    CarId = Car.CarId,
                    CarName = Car.Name,
                    DriverName = DriverName,
                    Color = SelectedColor,
                    StartDate = DateTime.Today,
                    Hours = Hours,
                    BasePrice = BasePrice,
                    Status = "Active"
                };
                CarDataService.Rentals.Add(rental);
                Car.Status = "Rented";

                MessageBox.Show(
                    $"🎉 Booking confirmed!\n\n{Car.Name} ({SelectedColor})\nDriver: {DriverName}\n{Hours} hour(s)  —  Total: ${TotalDue:F2}",
                    "Booking Confirmed", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.Navigate(new View.MyRentalsWindow(_userId));
            }, _ => CanConfirm);
        }
    }
}