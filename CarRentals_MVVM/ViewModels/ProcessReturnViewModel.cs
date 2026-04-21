using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class ProcessReturnViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }
        public ObservableCollection<RentalModel> ActiveRentals { get; } = new();

        private RentalModel? _selectedRental;
        public RentalModel? SelectedRental
        {
            get => _selectedRental;
            set { _selectedRental = value; OnPropertyChanged(); }
        }
        private int _actualHours = 1;
        public int ActualHours
        {
            get => _actualHours;
            set
            {
                if (value < 1) _actualHours = 1;
                else _actualHours = value;
                OnPropertyChanged();
            }
        }

        public ICommand BackCommand { get; }
        public ICommand ReturnCommand { get; }

        public ProcessReturnViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_userId)));

            ReturnCommand = new AsyncRelayCommand(async _ =>
            {
                if (SelectedRental == null)
                {
                    MessageBox.Show("Please select a rental first.",
                        "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (ActualHours <= 0)
                {
                    MessageBox.Show("Please enter valid actual hours.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string timeNote = ActualHours < SelectedRental.Hours
                    ? $"Early return — 10% discount on unused {SelectedRental.Hours - ActualHours} hour(s)."
                    : ActualHours > SelectedRental.Hours
                        ? $"Late return — charged for extra {ActualHours - SelectedRental.Hours} hour(s)."
                        : "On time return.";

                var confirm = MessageBox.Show(
                    $"Return {SelectedRental.CarName}?\n{timeNote}\nPlanned: {SelectedRental.Hours}h  |  Actual: {ActualHours}h",
                    "Confirm Return", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    var (finalAmount, returnStatus) =
                        await CarDataService.ProcessReturn(SelectedRental.RentalId, ActualHours);

                    SelectedRental.TotalAmount = finalAmount;
                    SelectedRental.Status = returnStatus;

                    CarDataService.GenerateReturnReport(SelectedRental);
                    ActiveRentals.Remove(SelectedRental);

                    MessageBox.Show(
                        $"Return processed!\nStatus: {returnStatus}\nFinal Amount: ${finalAmount:F2}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Return failed: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Load active rentals from SQL on open
            Task.Run(async () =>
            {
                var all = await CarDataService.GetAllRentals();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ActiveRentals.Clear();
                    foreach (var r in all)
                        if (r.Status == "Active") ActiveRentals.Add(r);
                });
            });
        }
    }
}