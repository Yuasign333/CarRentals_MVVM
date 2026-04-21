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
        /// 

        private readonly string[] _allowedColors = { "White", "Black", "Gray", "Blue", "Red" }; // only available colors of the company

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


                    // Convert the array to a comma-separated string for the TextBox
                    NewAvailableColors = _selectedCar.AvailableColors != null
                        ? string.Join(", ", _selectedCar.AvailableColors)
                        : string.Empty;
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

        private string _newAvailableColors = string.Empty; // Fixed!
        public string NewAvailableColors
        {
            get => _newAvailableColors;
            set
            {
                _newAvailableColors = value; OnPropertyChanged(nameof(NewAvailableColors));
            }
        }

        private string _newImageUrl = string.Empty; // Fixed!

        public string NewImageUrl
        {
            get => _newImageUrl;
            set
            {
                _newImageUrl = value; OnPropertyChanged(nameof(NewImageUrl));
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
        /// Updates Car Information 
        /// </summary>

        public ICommand UpdateCommand { get; }

        /// <summary>
        /// Resets all form fields and clears the table selection.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Initializes the Add Car ViewModel for the given admin user.
        /// Loads the existing car list and sets up all commands.
        /// </summary>
        /// <param name="userId">The logged-in admin's user ID.</param>
        public  AddCarViewModel(string userId)
        {

            _userId = userId;
            UserLabel = $"Agent: {userId}";

            Task.Run(async () =>
            {
                var allCars = await CarDataService.GetAll();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CarList.Clear();
                    foreach (var car in allCars)
                    {
                        CarList.Add(car);
                    }

                    // CRITICAL: Tell the UI to refresh the ID display now that cars are loaded
                    OnPropertyChanged(nameof(NextCarId));
                });
            });


            // Navigate back to the admin dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AdminDashboard(_userId));
            });


            // Save command — validate inputs then create and store a new car in Database
            // Save command — validate inputs then create and store a new car in Database
            SaveCommand = new AsyncRelayCommand(async _ =>
            {
                try
                {
                    // 1. Basic Validations
                    if (string.IsNullOrWhiteSpace(NewName))
                    {
                        MessageBox.Show("Please enter a car name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(NewAvailableColors))
                    {
                        MessageBox.Show("Please enter at least one available color.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // --- IDIOT PROOF 1: PREVENT DUPLICATE CAR NAMES ---
                    bool carAlreadyExists = CarList.Any(c => c.Name != null && c.Name.Equals(NewName, StringComparison.OrdinalIgnoreCase));

                    if (carAlreadyExists)
                    {
                        MessageBox.Show($"'{NewName}' is already in the system.\n\nIf you are trying to change the status or details of an existing car, please click 'Update' instead of 'Save'.",
                                        "Duplicate Car Prevented", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Split the text by commas and clean up spaces (removed .Distinct() so we can catch their mistakes)
                    var inputColors = NewAvailableColors.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(c => c.Trim())
                                                        .ToArray();

                    // --- IDIOT PROOF 2: CATCH DUPLICATE COLORS & SHOW ERROR ---
                    var duplicateColors = inputColors.GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
                                                     .Where(g => g.Count() > 1)
                                                     .Select(g => g.Key)
                                                     .ToList();

                    if (duplicateColors.Any())
                    {
                        MessageBox.Show($"You entered the same color multiple times: {string.Join(", ", duplicateColors)}.\n\nPlease list each color only once.",
                                        "Duplicate Colors Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; // Stops the save process
                    }

                    // Check if any typed colors are NOT in the allowed list
                    var invalidColors = inputColors.Where(c => !_allowedColors.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();

                    if (invalidColors.Any())
                    {
                        MessageBox.Show($"Invalid colors entered: {string.Join(", ", invalidColors)}\n\nAllowed colors are ONLY: White, Black, Gray, Blue, Red.",
                                        "Invalid Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Format colors nicely (e.g., "white" becomes "White")
                    string[] finalColorsArray = inputColors.Select(c => char.ToUpper(c[0]) + c.Substring(1).ToLower()).ToArray();

                    // 2. Build the new car object
                    var newCar = new CarModel
                    {
                        CarId = GenerateNextId(),
                        Name = NewName,
                        Category = NewCategory,
                        FuelType = NewFuelType,
                        PricePerHour = NewPricePerHour,
                        Status = NewStatus,
                        ImageUrl = NewImageUrl,
                        AvailableColors = finalColorsArray
                    };

                    // 3. Save to database 
                    await CarDataService.AddCar(newCar);

                    // 4. Add to the UI list
                    CarList.Add(newCar);

                    // 5. Clear form and show success
                    ExecuteClear();
                    MessageBox.Show("Car saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error saving car: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });


            // Delete command — remove the selected car from the database and the UI
            DeleteCommand = new AsyncRelayCommand(async _ =>
            {
                //  Check if a car is actually selected
                if (SelectedCar == null)
                {
                    MessageBox.Show("Please select a car from the list to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Business Logic: Prevent deleting cars that are in use
                if (SelectedCar.Status == "Rented")
                {
                    MessageBox.Show("Cannot delete a car that is currently rented.", "Invalid Action", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCar.Status == "Maintenance")
                {
                    MessageBox.Show("Cannot delete a car that is currently under maintenance.", "Invalid Action", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;

                }

                // Confirm with the user
                var result = MessageBox.Show($"Are you sure you want to permanently delete {SelectedCar.Name} (ID: {SelectedCar.CarId})?",
                                             "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 4. THE DATABASE PART: Delete from SQL first
                        await CarDataService.DeleteCar(SelectedCar.CarId);

                        // 5. THE UI PART: Only remove from list if DB deletion succeeded
                        CarList.Remove(SelectedCar);

                        // Clear the form fields
                        ExecuteClear();

                        MessageBox.Show("Car deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Error deleting car: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
            });

            UpdateCommand = new AsyncRelayCommand(async _ =>
            {
                if (SelectedCar == null)
                {
                    MessageBox.Show("Please select a car from the list to update.", "Selection Required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewAvailableColors))
                {
                    MessageBox.Show("Please enter at least one available color.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Split and Validate Colors (removed .Distinct())
                var inputColors = NewAvailableColors.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(c => c.Trim())
                                                    .ToArray();

                // --- CATCH DUPLICATE COLORS IN UPDATE ---
                var duplicateColors = inputColors.GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key)
                                                 .ToList();

                if (duplicateColors.Any())
                {
                    MessageBox.Show($"You entered the same color multiple times: {string.Join(", ", duplicateColors)}.\n\nPlease list each color only once.",
                                    "Duplicate Colors Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Stops the update process
                }

                // Catch invalid colors
                var invalidColors = inputColors.Where(c => !_allowedColors.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
                if (invalidColors.Any())
                {
                    MessageBox.Show($"Invalid colors entered: {string.Join(", ", invalidColors)}\n\nAllowed colors are ONLY: White, Black, Gray, Blue, Red.",
                                    "Invalid Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string[] finalColorsArray = inputColors.Select(c => char.ToUpper(c[0]) + c.Substring(1).ToLower()).ToArray();

                // Map the form fields back to a temporary object
                var updatedCar = new CarModel
                {
                    CarId = SelectedCar.CarId,
                    Name = NewName,
                    Category = NewCategory,
                    FuelType = NewFuelType,
                    PricePerHour = NewPricePerHour,
                    Status = NewStatus,
                    ImageUrl = NewImageUrl,
                    AvailableColors = finalColorsArray
                };

                try
                {
                    // 1. Send the update to the database 
                    await CarDataService.UpdateCar(updatedCar);

                    // 2. Update the UI list 
                    var existingCar = CarList.FirstOrDefault(c => c.CarId == updatedCar.CarId);
                    if (existingCar != null)
                    {
                        int index = CarList.IndexOf(existingCar);
                        CarList[index] = updatedCar;
                    }

                    // 3. Clear form and show success
                    ExecuteClear();
                    MessageBox.Show("Car updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error updating car: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Clear command — reset the form without saving or deleting
            ClearCommand = new RelayCommand(_ =>
            {
                ExecuteClear();

                // Inform the user that the form has been cleared
                MessageBox.Show(
                    "Form cleared.",
                    "Cleared",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
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
            // Look at CarList (the one bound to your UI) instead of the Service
            if (CarList == null || !CarList.Any())
            {
                return "C001";
            }

            // Find the highest numeric suffix among currently loaded cars
            var maxNumber = CarList
                .Select(c => c.CarId)
                .Where(id => id != null && id.StartsWith("C") && id.Length > 1)
                .Select(id => int.TryParse(id.Substring(1), out int number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max();

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
            NewAvailableColors = string.Empty;
            SelectedCar = null;

            // Refresh the auto-generated ID display after any list change
            OnPropertyChanged(nameof(NextCarId));
        }
    }
}