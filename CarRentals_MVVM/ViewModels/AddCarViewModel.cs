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
    /// ViewModel for AddCarWindow.xaml.
    /// Handles the admin's car management screen — viewing the fleet list,
    /// adding new cars, deleting existing cars, and clearing the form.
    /// Connected to: AddCarWindow.xaml (View),
    /// AddCarWindow.xaml.cs (sets DataContext to this ViewModel),
    /// CarDataService (reads and modifies the car list),
    /// AdminDashboard (navigates here on Back).
    /// </summary>
    public class AddCarViewModel : ObservableObject
    {
        // The logged-in admin's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right of the window (e.g. "Agent: A001").
        /// Bound to the user badge TextBlock in AddCarWindow.xaml.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// The live list of all cars displayed in the left-side table.
        /// Stays in sync with CarDataService.Cars as cars are added or deleted.
        /// Bound to the ListView in AddCarWindow.xaml.
        /// </summary>
        public ObservableCollection<CarModel> CarList { get; set; } = new();

        // ── Selected car (for editing and deletion) ────────────────────────────

        private CarModel? _selectedCar;

        /// <summary>
        /// The car currently selected in the left table.
        /// When set, automatically populates the form fields on the right
        /// so the admin can see the car's current data.
        /// Bound to the ListView SelectedItem in AddCarWindow.xaml.
        /// </summary>
        public CarModel? SelectedCar
        {
            get => _selectedCar;
            set
            {
                _selectedCar = value;
                OnPropertyChanged();

                // Populate form fields when a car is selected from the list
                if (_selectedCar != null)
                {
                    NewName = _selectedCar.Name;
                    NewCategory = _selectedCar.Category;
                    NewFuelType = _selectedCar.FuelType;
                    NewPricePerHour = _selectedCar.PricePerHour;
                    NewStatus = _selectedCar.Status;
                    NewImageUrl = _selectedCar.ImageUrl;
                }
            }
        }

        // ── Auto-generated Car ID ──────────────────────────────────────────────

        /// <summary>
        /// Read-only computed ID shown in the Car ID field on the form.
        /// Calls GenerateNextId() every time it is read.
        /// Bound to the Car ID display TextBlock in AddCarWindow.xaml.
        /// </summary>
        public string NextCarId => GenerateNextId();

        // ── Form input fields ──────────────────────────────────────────────────

        private string _newName = string.Empty;

        /// <summary>
        /// The car name entered in the form (e.g. "Toyota Camry").
        /// Bound to the Car Name TextBox in AddCarWindow.xaml.
        /// </summary>
        public string NewName
        {
            get => _newName;
            set
            {
                _newName = value;
                OnPropertyChanged();
            }
        }

        private string _newCategory = string.Empty;

        /// <summary>
        /// The category selected in the form ("Sedan", "SUV", "Van").
        /// Bound to the Category ComboBox in AddCarWindow.xaml.
        /// </summary>
        public string NewCategory
        {
            get => _newCategory;
            set
            {
                _newCategory = value;
                OnPropertyChanged();
            }
        }

        private string _newFuelType = string.Empty;

        /// <summary>
        /// The fuel type selected in the form ("Standard Engine", "EV", "Hybrid Engine").
        /// Bound to the Fuel Type ComboBox in AddCarWindow.xaml.
        /// </summary>
        public string NewFuelType
        {
            get => _newFuelType;
            set
            {
                _newFuelType = value;
                OnPropertyChanged();
            }
        }

        private decimal _newPricePerHour;

        /// <summary>
        /// The price per hour entered in the form.
        /// Must be greater than 0 — validated in SaveCommand.
        /// Bound to the Price Per Hour TextBox in AddCarWindow.xaml.
        /// </summary>
        public decimal NewPricePerHour
        {
            get => _newPricePerHour;
            set
            {
                _newPricePerHour = value;
                OnPropertyChanged();
            }
        }

        private string _newStatus = string.Empty;

        /// <summary>
        /// The status selected in the form ("Available", "Rented", "Maintenance").
        /// Bound to the Status ComboBox in AddCarWindow.xaml.
        /// </summary>
        public string NewStatus
        {
            get => _newStatus;
            set
            {
                _newStatus = value;
                OnPropertyChanged();
            }
        }

        private string _newImageUrl = string.Empty;

        /// <summary>
        /// An optional direct image URL for the car (e.g. an Imgur link).
        /// If left empty, the car will display a placeholder gradient in the browse view.
        /// Bound to the Image URL TextBox in AddCarWindow.xaml.
        /// </summary>
        public string NewImageUrl
        {
            get => _newImageUrl;
            set
            {
                _newImageUrl = value;
                OnPropertyChanged();
            }
        }

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>Navigates back to AdminDashboard.</summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Validates all form fields and adds a new car to the fleet.
        /// On success: adds to CarDataService.Cars and CarList, then clears the form.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Deletes the currently selected car from the fleet.
        /// Requires a car to be selected in the table first.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Resets all form fields and clears the table selection.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Initializes the Add Car ViewModel for the given admin user.
        /// Loads the existing car list and sets up all commands.
        /// </summary>
        /// <param name="userId">The logged-in admin's user ID.</param>
        public AddCarViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            // Load all existing cars into the observable list for the table
            foreach (var car in CarDataService.Cars)
            {
                CarList.Add(car);
            }

            // Navigate back to the admin dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AdminDashboard(_userId));
            });

            // Save command — validate inputs then create and store a new car
            SaveCommand = new RelayCommand(_ =>
            {
                // Car name is required
                if (string.IsNullOrWhiteSpace(NewName))
                {
                    MessageBox.Show(
                        "Car Name is required.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Check for duplicate car name (case-insensitive)
                bool isDuplicate = false;

                foreach (var car in CarDataService.Cars)
                {
                    if (car.Name.ToLower() == NewName.ToLower())
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (isDuplicate)
                {
                    MessageBox.Show(
                        $"A car named '{NewName}' already exists.",
                        "Duplicate",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Category is required
                if (string.IsNullOrWhiteSpace(NewCategory))
                {
                    MessageBox.Show(
                        "Please select a Category.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Fuel type is required
                if (string.IsNullOrWhiteSpace(NewFuelType))
                {
                    MessageBox.Show(
                        "Please select a Fuel Type.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Status is required
                if (string.IsNullOrWhiteSpace(NewStatus))
                {
                    MessageBox.Show(
                        "Please select a Status.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Price must be greater than zero
                if (NewPricePerHour <= 0)
                {
                    MessageBox.Show(
                        "Price Per Hour must be greater than 0.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Build the new car object from the form data
                var newCar = new CarModel
                {
                    CarId = GenerateNextId(),
                    Name = NewName,
                    Category = NewCategory,
                    FuelType = NewFuelType,
                    PricePerHour = NewPricePerHour,
                    Status = NewStatus,
                    ImageUrl = NewImageUrl,

                    // Default color options for all newly added cars
                    AvailableColors = ["White", "Black", "Silver"]
                };

                // Add to both the data store and the visible table list
                CarDataService.Cars.Add(newCar);
                CarList.Add(newCar);

                MessageBox.Show(
                    $"Car added with ID: {newCar.CarId}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Reset the form after successful save
                ExecuteClear();
            });

            // Delete command — remove the selected car from the fleet
            DeleteCommand = new RelayCommand(_ =>
            {
                if (SelectedCar == null)
                {
                    MessageBox.Show(
                        "Please select a car to delete.",
                        "No Selection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Remove from both the data store and the visible table list
                CarDataService.Cars.Remove(SelectedCar);
                CarList.Remove(SelectedCar);

                // Reset form after deletion
                ExecuteClear();
            });

            // Clear command — reset the form without saving or deleting
            ClearCommand = new RelayCommand(_ =>
            {
                ExecuteClear();
            });
        }

        /// <summary>
        /// Generates the next available car ID by finding the highest
        /// existing C-number and adding 1.
        /// Deleted car IDs are never reused — the counter always goes forward.
        /// Example: if C001, C003 exist (C002 was deleted), next ID = C004.
        /// </summary>
        private string GenerateNextId()
        {
            // If no cars exist yet, start from C001
            if (!CarDataService.Cars.Any())
            {
                return "C001";
            }

            // Find the highest numeric suffix among all existing car IDs
            var maxNumber = CarDataService.Cars
                .Select(c => c.CarId)
                .Where(id => id.StartsWith("C") && id.Length > 1)
                .Select(id => int.TryParse(id.Substring(1), out int number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max();

            // Return the next ID formatted as C followed by 3 digits (e.g. C007)
            return $"C{(maxNumber + 1):D3}";
        }

        /// <summary>
        /// Resets all form input fields and clears the table selection.
        /// Also refreshes NextCarId so the new auto-generated ID is shown.
        /// Called after Save, Delete, or Clear button is triggered.
        /// </summary>
        private void ExecuteClear()
        {
            NewName = string.Empty;
            NewCategory = string.Empty;
            NewFuelType = string.Empty;
            NewPricePerHour = 0;
            NewStatus = string.Empty;
            NewImageUrl = string.Empty;
            SelectedCar = null;

            // Refresh the auto-generated ID display after any list change
            OnPropertyChanged(nameof(NextCarId));
        }
    }
}