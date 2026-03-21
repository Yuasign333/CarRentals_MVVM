using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class AddCarViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }

        public ObservableCollection<CarModel> CarList { get; set; } = new();

        private CarModel? _selectedCar;
        public CarModel? SelectedCar
        {
            get => _selectedCar;
            set
            {
                _selectedCar = value;
                OnPropertyChanged();
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

        // CarId is now read-only display, auto-generated
        public string NextCarId => GenerateNextId();

        private string _newName = string.Empty;
        public string NewName
        {
            get => _newName;
            set { _newName = value; OnPropertyChanged(); }
        }

        private string _newCategory = string.Empty;
        public string NewCategory
        {
            get => _newCategory;
            set { _newCategory = value; OnPropertyChanged(); }
        }

        private string _newFuelType = string.Empty;
        public string NewFuelType
        {
            get => _newFuelType;
            set { _newFuelType = value; OnPropertyChanged(); }
        }

        private decimal _newPricePerHour;
        public decimal NewPricePerHour
        {
            get => _newPricePerHour;
            set { _newPricePerHour = value; OnPropertyChanged(); }
        }

        private string _newStatus = string.Empty;
        public string NewStatus
        {
            get => _newStatus;
            set { _newStatus = value; OnPropertyChanged(); }
        }

        private string _newImageUrl = string.Empty;
        public string NewImageUrl
        {
            get => _newImageUrl;
            set { _newImageUrl = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public AddCarViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            foreach (var car in CarDataService.Cars)
                CarList.Add(car);

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_userId)));
            SaveCommand = new RelayCommand(_ =>
            {
                // Name required
                if (string.IsNullOrWhiteSpace(NewName))
                {
                    MessageBox.Show("Car Name is required.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Category required
                if (string.IsNullOrWhiteSpace(NewCategory))
                {
                    MessageBox.Show("Please select a Category.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Fuel Type required
                if (string.IsNullOrWhiteSpace(NewFuelType))
                {
                    MessageBox.Show("Please select a Fuel Type.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Status required
                if (string.IsNullOrWhiteSpace(NewStatus))
                {
                    MessageBox.Show("Please select a Status.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Price must be more than 0
                if (NewPricePerHour <= 0)
                {
                    MessageBox.Show("Price Per Hour must be greater than 0.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newCar = new CarModel
                {
                    CarId = GenerateNextId(),
                    Name = NewName,
                    Category = NewCategory,
                    FuelType = NewFuelType,
                    PricePerHour = NewPricePerHour,
                    Status = NewStatus,
                    ImageUrl = NewImageUrl,
                    AvailableColors = ["White", "Black", "Silver"]
                };

                CarDataService.Cars.Add(newCar);
                CarList.Add(newCar);

                MessageBox.Show($"Car added with ID: {newCar.CarId}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ExecuteClear();
            });

            DeleteCommand = new RelayCommand(_ =>
            {
                if (SelectedCar == null)
                {
                    MessageBox.Show("Please select a car to delete.", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                CarDataService.Cars.Remove(SelectedCar);
                CarList.Remove(SelectedCar);
                ExecuteClear();
            });

            ClearCommand = new RelayCommand(_ => ExecuteClear());
        }

        // Auto-increment: finds highest C-number and adds 1
        private string GenerateNextId()
        {
            if (!CarDataService.Cars.Any())
                return "C001";

            var max = CarDataService.Cars
                .Select(c => c.CarId)
                .Where(id => id.StartsWith("C") && id.Length > 1)
                .Select(id => int.TryParse(id.Substring(1), out int n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"C{(max + 1):D3}";
        }

        private void Pricemorethan0()
        {
            
        }
        private void ExecuteClear()
        {
            NewName = string.Empty;
            NewCategory = string.Empty;
            NewFuelType = string.Empty;
            NewPricePerHour = 0;
            NewStatus = string.Empty;
            NewImageUrl = string.Empty;
            SelectedCar = null;
            OnPropertyChanged(nameof(NextCarId));
        }
    }
}