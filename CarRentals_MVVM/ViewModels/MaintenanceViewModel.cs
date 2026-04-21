using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class MaintenanceViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }
        public ObservableCollection<MaintenanceModel> MaintenanceList { get; } = new();
        public ObservableCollection<CarModel> CarList { get; } = new();

        private MaintenanceModel? _selectedMaintenance;
        public MaintenanceModel? SelectedMaintenance
        {
            get => _selectedMaintenance;
            set { _selectedMaintenance = value; OnPropertyChanged(); }
        }

        // Form fields
        private string _selectedCarId = string.Empty;
        public string SelectedCarId
        {
            get => _selectedCarId;
            set { _selectedCarId = value; OnPropertyChanged(); }
        }

        private string _technicianName = string.Empty;
        public string TechnicianName
        {
            get => _technicianName;

            set 
            {
                // if user tries to enter numbers or special characters, show error message and do not update the field
                if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsDigit) || value.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)))
                {
                    MessageBox.Show("Please enter a valid technician name (letters and spaces only).", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
              

                else
                    _technicianName = value; OnPropertyChanged(); 
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private decimal _completionCost = 0;
        public decimal CompletionCost
        {
            get => _completionCost;
            set {
                // if user tries to enter a negative number, show error message and do not update the field
                if (value < 0)
                {
                    MessageBox.Show("Please enter a valid cost (non-negative number).", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                    _completionCost = value; OnPropertyChanged(); 
            }
        }

        public ICommand BackCommand { get; }
        public ICommand SendToMaintCommand { get; }
        public ICommand CompleteCommand { get; }

        public MaintenanceViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_userId)));

            // Send car to maintenance
            SendToMaintCommand = new AsyncRelayCommand(async _ =>
            {
                if (string.IsNullOrWhiteSpace(SelectedCarId) ||
                    string.IsNullOrWhiteSpace(TechnicianName))
                {
                    MessageBox.Show("Please select a car and enter a technician name.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var car = await CarDataService.GetById(SelectedCarId);
                    if (car == null)
                    {
                        MessageBox.Show("Car not found.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (car.Status == "Rented")
                    {
                        MessageBox.Show("Cannot send a rented car to maintenance.",
                            "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (car.Status == "Maintenance")
                    {
                        MessageBox.Show("This car is already in maintenance.",
                            "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string maintId = await CarDataService.GetNextMaintenanceId();
                    var record = new MaintenanceModel
                    {
                        MaintenanceId = maintId,
                        CarId = SelectedCarId,
                        TechnicianName = TechnicianName,
                        Description = Description,
                        StartDate = DateTime.Now,
                        Status = "In Progress"
                    };

                    // Save maintenance record to SQL
                    await CarDataService.SaveMaintenance(record);

                    // Update car status to Maintenance in SQL
                    car.Status = "Maintenance";
                    await CarDataService.UpdateCar(car);

                    MaintenanceList.Add(record);

                    MessageBox.Show($"Car {SelectedCarId} sent to maintenance. ID: {maintId}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Generate maintenance log file
                    var carForLog = await CarDataService.GetById(SelectedCarId);
                    CarDataService.GenerateMaintenanceLog(record, carForLog?.Name ?? SelectedCarId);

                    SelectedCarId = string.Empty;
                    TechnicianName = string.Empty;
                    Description = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Complete maintenance
            CompleteCommand = new AsyncRelayCommand(async _ =>
            {
                if (SelectedMaintenance == null)
                {
                    MessageBox.Show("Please select a maintenance record.",
                        "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Mark maintenance complete in SQL
                    await CarDataService.CompleteMaintenance(
                        SelectedMaintenance.MaintenanceId, CompletionCost);

                    // Set car back to Available in SQL
                    var car = await CarDataService.GetById(SelectedMaintenance.CarId);
                    if (car != null)
                    {
                        car.Status = "Available";
                        await CarDataService.UpdateCar(car);
                    }

                    // Refresh list
                    SelectedMaintenance.Status = "Completed";
                    SelectedMaintenance.EndDate = DateTime.Now;
                    SelectedMaintenance.Cost = CompletionCost;

                    MessageBox.Show("Maintenance completed. Car is now Available.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CompletionCost = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Load data
            Task.Run(async () =>
            {
                var mList = await CarDataService.GetAllMaintenance();
                var cList = await CarDataService.GetAll();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MaintenanceList.Clear();
                    foreach (var m in mList) MaintenanceList.Add(m);

                    CarList.Clear();
                    foreach (var c in cList) CarList.Add(c);
                });
            });
        }
    }
}