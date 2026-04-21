using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for MaintenanceWindow.xaml.
    /// Handles two admin functions:
    ///   1. Send a car to maintenance — creates a maintenance record and sets car status to "Maintenance".
    ///   2. Complete maintenance — marks the record as Completed and sets car back to "Available".
    /// Connected to: MaintenanceWindow.xaml (View),
    /// MaintenanceWindow.xaml.cs (sets DataContext),
    /// CarDataService (reads cars, saves/completes maintenance, updates car status),
    /// AdminDashboard (navigates back here on Back).
    /// </summary>
    public class MaintenanceViewModel : ObservableObject
    {
        // The logged-in admin's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge (e.g. "Agent: A001").
        /// Bound to the user badge TextBlock in MaintenanceWindow.xaml.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// The list of all maintenance records loaded from the database.
        /// Bound to the maintenance table ListView in MaintenanceWindow.xaml.
        /// </summary>
        public ObservableCollection<MaintenanceModel> MaintenanceList { get; } = new();

        /// <summary>
        /// The list of all cars loaded from the database.
        /// Used to populate the car selection dropdown in the form.
        /// Bound to the Car ComboBox in MaintenanceWindow.xaml.
        /// </summary>
        public ObservableCollection<CarModel> CarList { get; } = new();

        // ── Selected maintenance record (for CompleteCommand) ──────────────────

        private MaintenanceModel? _selectedMaintenance;

        /// <summary>
        /// The maintenance record currently selected in the table.
        /// Required before CompleteCommand can run.
        /// Bound to the ListView SelectedItem in MaintenanceWindow.xaml.
        /// </summary>
        public MaintenanceModel? SelectedMaintenance
        {
            get => _selectedMaintenance;
            set
            {
                _selectedMaintenance = value;
                OnPropertyChanged();
            }
        }

        // ── Form fields (for SendToMaintCommand) ──────────────────────────────

        private string _selectedCarId = string.Empty;

        /// <summary>
        /// The CarId selected from the dropdown for sending to maintenance.
        /// Bound to the Car ComboBox SelectedValue in MaintenanceWindow.xaml.
        /// </summary>
        public string SelectedCarId
        {
            get => _selectedCarId;
            set
            {
                _selectedCarId = value;
                OnPropertyChanged();
            }
        }

        private string _technicianName = string.Empty;

        /// <summary>
        /// The technician's name entered in the form.
        /// Only accepts letters and spaces — rejects numbers and special characters.
        /// Bound to the Technician Name TextBox in MaintenanceWindow.xaml.
        /// </summary>
        public string TechnicianName
        {
            get => _technicianName;
            set
            {
                // Reject names containing numbers or special characters
                if (string.IsNullOrWhiteSpace(value) ||
                    value.Any(char.IsDigit) ||
                    value.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)))
                {
                    MessageBox.Show(
                        "Please enter a valid technician name (letters and spaces only).",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _technicianName = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty;

        /// <summary>
        /// The maintenance description entered in the form.
        /// Bound to the Description TextBox in MaintenanceWindow.xaml.
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private decimal _completionCost = 0;

        /// <summary>
        /// The cost of completing maintenance entered when running CompleteCommand.
        /// Must be a non-negative number.
        /// Bound to the Completion Cost TextBox in MaintenanceWindow.xaml.
        /// </summary>
        public decimal CompletionCost
        {
            get => _completionCost;
            set
            {
                // Reject negative cost values
                if (value < 0)
                {
                    MessageBox.Show(
                        "Please enter a valid cost (non-negative number).",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _completionCost = value;
                OnPropertyChanged();
            }
        }

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>Navigates back to AdminDashboard.</summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Sends the selected car to maintenance.
        /// Validates inputs, creates a MaintenanceModel, saves to DB,
        /// updates car status to "Maintenance", and generates a maintenance log file.
        /// </summary>
        public ICommand SendToMaintCommand { get; }

        /// <summary>
        /// Marks the selected maintenance record as Completed.
        /// Updates the record in DB with EndDate and Cost,
        /// and sets the car status back to "Available".
        /// </summary>
        public ICommand CompleteCommand { get; }

        /// <summary>
        /// Initializes the Maintenance ViewModel for the given admin.
        /// </summary>
        /// <param name="userId">The logged-in admin's user ID.</param>
        public MaintenanceViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            // Navigate back to the admin dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AdminDashboard(_userId));
            });

            // Send a car to maintenance
            SendToMaintCommand = new AsyncRelayCommand(async _ =>
            {
                // Both car and technician are required
                if (string.IsNullOrWhiteSpace(SelectedCarId) ||
                    string.IsNullOrWhiteSpace(TechnicianName))
                {
                    MessageBox.Show(
                        "Please select a car and enter a technician name.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
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

                    // Cannot send a currently rented car to maintenance
                    if (car.Status == "Rented")
                    {
                        MessageBox.Show(
                            "Cannot send a rented car to maintenance.",
                            "Invalid",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Car is already in maintenance
                    if (car.Status == "Maintenance")
                    {
                        MessageBox.Show(
                            "This car is already in maintenance.",
                            "Invalid",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Generate maintenance ID and build the record
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

                    // Update the car's status to Maintenance in SQL
                    car.Status = "Maintenance";
                    await CarDataService.UpdateCar(car);

                    // Add to the visible list and show success
                    MaintenanceList.Add(record);

                    MessageBox.Show(
                        $"Car {SelectedCarId} sent to maintenance. ID: {maintId}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Generate a maintenance log .txt file in Admin/MaintenanceLogs/
                    var carForLog = await CarDataService.GetById(SelectedCarId);
                    CarDataService.GenerateMaintenanceLog(record, carForLog?.Name ?? SelectedCarId);

                    // Reset the form fields
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

            // Complete a maintenance record
            CompleteCommand = new AsyncRelayCommand(async _ =>
            {
                // A record must be selected from the table
                if (SelectedMaintenance == null)
                {
                    MessageBox.Show(
                        "Please select a maintenance record.",
                        "No Selection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Mark the maintenance record as Completed in SQL
                    await CarDataService.CompleteMaintenance(
                        SelectedMaintenance.MaintenanceId, CompletionCost);

                    // Set the car back to Available in SQL
                    var car = await CarDataService.GetById(SelectedMaintenance.CarId);

                    if (car != null)
                    {
                        car.Status = "Available";
                        await CarDataService.UpdateCar(car);
                    }

                    // Update the selected record's properties so the UI reflects the change
                    SelectedMaintenance.Status = "Completed";
                    SelectedMaintenance.EndDate = DateTime.Now;
                    SelectedMaintenance.Cost = CompletionCost;

                    MessageBox.Show(
                        "Maintenance completed. Car is now Available.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Reset the completion cost field
                    CompletionCost = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Load maintenance records and car list from DB on window open
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