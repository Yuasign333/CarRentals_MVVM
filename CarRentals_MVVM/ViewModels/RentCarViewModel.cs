// ─────────────────────────────────────────────────────────────────────────────
// ViewModel for RentCarWindow.xaml.
// Handles the 2-step rental process for a specific car:
//   Step 1 — Select a color from the car's available options
//   Step 2 — Fill in driver name, hours, and confirm the booking
// Note: This window is still in the project but the main rental flow now
// lives in BrowseCarsViewModel (3-page system). RentCarWindow is kept
// as a fallback/reference.
// Connected to: RentCarWindow.xaml (View),
// RentCarWindow.xaml.cs (sets DataContext to this ViewModel),
// CarDataService (saves the completed rental),
// MyRentalsWindow (navigated to after booking confirmed),
// BrowseCarsWindow (navigated to on Back).
// ─────────────────────────────────────────────────────────────────────────────

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
        // The logged-in customer's user ID — used when navigating
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge (e.g. "Customer: C001").
        /// Bound to the user badge TextBlock in RentCarWindow.xaml.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// The car being rented — passed in from BrowseCarsWindow
        /// when the customer clicks a car card.
        /// Used to display car details and compute prices.
        /// </summary>
        public CarModel Car { get; }

        // ── Step switching ─────────────────────────────────────────────────────

        private int _step = 1;

        /// <summary>
        /// The current step of the rental process (1 or 2).
        /// Changing this notifies IsStep1 and IsStep2 so the correct
        /// section becomes visible in RentCarWindow.xaml.
        /// </summary>
        public int Step
        {
            get => _step;
            set
            {
                _step = value;
                OnPropertyChanged();

                // Notify both step flags so the XAML visibility bindings update
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
            }
        }

        /// <summary>
        /// True when Step 1 (color picker) should be visible.
        /// Bound to the Visibility of the Step 1 section in RentCarWindow.xaml.
        /// </summary>
        public bool IsStep1 => Step == 1;

        /// <summary>
        /// True when Step 2 (booking form) should be visible.
        /// Bound to the Visibility of the Step 2 section in RentCarWindow.xaml.
        /// </summary>
        public bool IsStep2 => Step == 2;

        // ── Step 1 — Color selection ───────────────────────────────────────────

        private string _selectedColor = string.Empty;

        /// <summary>
        /// The color the customer selected from the car's available colors.
        /// Defaults to the first color in the car's AvailableColors list.
        /// Bound to the color ListBox SelectedItem in RentCarWindow.xaml.
        /// Saved to RentalModel.Color when booking is confirmed.
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

        // ── Step 2 — Booking details ───────────────────────────────────────────

        private string _driverName = string.Empty;

        /// <summary>
        /// The name of the person who will be driving the car.
        /// Required — ConfirmCommand is disabled if this is empty.
        /// Also notifies CanConfirm so the Confirm button enables/disables.
        /// Bound to the Driver Name TextBox in RentCarWindow.xaml.
        /// </summary>
        public string DriverName
        {
            get => _driverName;
            set
            {
                _driverName = value;
                OnPropertyChanged();

                // Re-evaluate whether the Confirm button should be enabled
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        private int _hours = 1;

        /// <summary>
        /// The number of hours the car will be rented.
        /// Minimum is 1 — values below 1 are clamped to 1.
        /// Updating this recalculates BasePrice and TotalDue.
        /// Bound to the Hours TextBox in RentCarWindow.xaml.
        /// </summary>
        public int Hours
        {
            get => _hours;
            set
            {
                // Prevent zero or negative hours — minimum rental is 1 hour
                if (value < 1)
                {
                    _hours = 1;
                }
                else
                {
                    _hours = value;
                }

                OnPropertyChanged();

                // Recalculate prices when hours change
                OnPropertyChanged(nameof(BasePrice));
                OnPropertyChanged(nameof(TotalDue));
            }
        }

        // ── Computed price properties ──────────────────────────────────────────

        /// <summary>
        /// Fixed security deposit of $50 for all rentals.
        /// Shown in the Rental Estimate box in RentCarWindow.xaml.
        /// </summary>
        public decimal Deposit => 50m;

        /// <summary>
        /// Base cost = PricePerHour x Hours.
        /// Recalculated automatically when Hours changes.
        /// Shown in the Rental Estimate box in RentCarWindow.xaml.
        /// </summary>
        public decimal BasePrice => Hours * Car.PricePerHour;

        /// <summary>
        /// Total amount due = BasePrice + Deposit.
        /// Shown in the Rental Estimate box and included in the
        /// confirmation message when booking is confirmed.
        /// </summary>
        public decimal TotalDue => BasePrice + Deposit;

        /// <summary>
        /// Returns true only when DriverName is not empty.
        /// Controls whether ConfirmCommand is enabled.
        /// Bound to the IsEnabled or CanExecute of the Confirm button.
        /// </summary>
        public bool CanConfirm => !string.IsNullOrWhiteSpace(DriverName);

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>
        /// Sets the SelectedColor when the customer clicks a color swatch.
        /// Receives the color string as the CommandParameter.
        /// </summary>
        public ICommand SelectColorCommand { get; }

        /// <summary>
        /// Moves from Step 1 to Step 2.
        /// Only advances if currently on Step 1.
        /// </summary>
        public ICommand NextCommand { get; }

        /// <summary>
        /// Moves back from Step 2 to Step 1.
        /// Only goes back if currently on Step 2.
        /// </summary>
        public ICommand BackStepCommand { get; }

        /// <summary>
        /// Validates the booking and creates the rental transaction.
        /// Disabled (CanConfirm = false) when DriverName is empty.
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// Navigates back to BrowseCarsWindow without saving anything.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the RentCarViewModel for the given customer and car.
        /// Sets up default values and all commands.
        /// </summary>
        /// <param name="userId">The logged-in customer's ID.</param>
        /// <param name="car">The car that was selected in BrowseCarsWindow.</param>
        public RentCarViewModel(string userId, CarModel car)
        {
            _userId = userId;
            Car = car;
            UserLabel = $"Customer: {userId}";

            // Default the selected color to the first option available for this car
            if (car.AvailableColors.Length > 0)
            {
                SelectedColor = car.AvailableColors[0];
            }
            else
            {
                SelectedColor = "White";
            }

            // Set the selected color when a color swatch is clicked
            // CommandParameter passes the color string from the XAML binding
            SelectColorCommand = new RelayCommand(colorParam =>
            {
                if (colorParam is string color)
                {
                    SelectedColor = color;
                }
            });

            // Advance to the booking form step
            NextCommand = new RelayCommand(_ =>
            {
                if (Step < 2)
                {
                    Step++;
                }
            });

            // Go back to the color picker step
            BackStepCommand = new RelayCommand(_ =>
            {
                if (Step > 1)
                {
                    Step--;
                }
            });

            // Navigate back to the car browse window without booking
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId));
            });

            // Confirm booking — validate, save rental, update car status, navigate
            // The second parameter (canExecute) disables the button when CanConfirm is false
            ConfirmCommand = new RelayCommand(
                execute: _ =>
                {
                    // Driver name is required before confirming
                    if (string.IsNullOrWhiteSpace(DriverName))
                    {
                        MessageBox.Show(
                            "Please enter a driver name.",
                            "Required",
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
                        CarId = Car.CarId,
                        CarName = Car.Name,
                        DriverName = DriverName,
                        Color = SelectedColor,
                        StartDate = DateTime.Today,
                        Hours = Hours,
                        BasePrice = BasePrice,
                        Status = "Active"
                    };

                    // Save the rental to the in-memory store
                    CarDataService.Rentals.Add(rental);

                    // Mark the car as Rented so it no longer appears as Available
                    Car.Status = "Rented";

                    // Show confirmation summary to the customer
                    MessageBox.Show(
                        $"Booking confirmed!\n\n" +
                        $"{Car.Name} ({SelectedColor})\n" +
                        $"Driver: {DriverName}\n" +
                        $"{Hours} hour(s)  —  Total: ${TotalDue:F2}",
                        "Booking Confirmed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Navigate to the customer's rental history
                    NavigationService.Navigate(new View.MyRentalsWindow(_userId));
                },
                canExecute: _ => CanConfirm
            );
        }
    }
}