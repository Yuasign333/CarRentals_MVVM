using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for processing car returns.
    /// Logic includes calculating price adjustments for early or late returns
    /// and updating the database to ensure revenue reports are accurate.
    /// </summary>
    public class ProcessReturnViewModel : ObservableObject
    {
        private readonly string _userId;

        /// <summary>Displays the current Admin/Agent ID in the view.</summary>
        public string UserLabel { get; }

        /// <summary>A filtered list containing only rentals currently marked as 'Active'.</summary>
        public ObservableCollection<RentalModel> ActiveRentals { get; } = new();

        private RentalModel? _selectedRental;
        /// <summary>The rental record currently highlighted in the UI list.</summary>
        public RentalModel? SelectedRental
        {
            get => _selectedRental;
            set { _selectedRental = value; OnPropertyChanged(); }
        }

        private int _actualHours = 1;
        /// <summary>
        /// The real duration the car was used. 
        /// This is compared against 'SelectedRental.Hours' to determine fees/discounts.
        /// </summary>
        public int ActualHours
        {
            get => _actualHours;
            set
            {
                _actualHours = value < 1 ? 1 : value;
                OnPropertyChanged();
            }
        }

        public ICommand BackCommand { get; }
        public ICommand ReturnCommand { get; }

        public ProcessReturnViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            // Navigation back to the main Admin Dashboard
            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_userId)));

            // The main execution logic for finalizing a rental
            ReturnCommand = new AsyncRelayCommand(async _ =>
            {
                if (SelectedRental == null) return;

                // ── 1. Calculate Adjustment Logic for User Confirmation ─────────
                // This section determines what message to show the agent before committing.
                string timeNote = "";
                if (ActualHours < SelectedRental.Hours)
                {
                    int unused = SelectedRental.Hours - ActualHours;
                    timeNote = $"Early return: {unused} hours unused (10% discount applied to these).";
                }
                else if (ActualHours > SelectedRental.Hours)
                {
                    int extra = ActualHours - SelectedRental.Hours;
                    timeNote = $"Late return: {extra} extra hours will be charged.";
                }
                else
                {
                    timeNote = "On-time return: No price adjustments.";
                }

                var confirm = MessageBox.Show(
                    $"{SelectedRental.CarName} Return Summary:\n\n{timeNote}\n\nProceed with payment update?",
                    "Confirm Final Settlement", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    // ── 2. DATABASE UPDATE ──────────────────────────────────────
                    // The CarDataService handles the heavy lifting:
                    // - Recalculates TotalAmount based on ActualHours
                    // - Updates the SQL 'Rentals' table Status and TotalAmount
                    // - Frees up the Car for future use (Status = 'Available')
                    var (finalAmount, returnStatus) = await CarDataService.ProcessReturn(SelectedRental.RentalId, ActualHours);

                    // ── 3. UI SYNC ──────────────────────────────────────────────
                    // Update the local object so the UI reflects the change immediately 
                    // before it is removed from the active list.
                    SelectedRental.TotalAmount = finalAmount;
                    SelectedRental.Status = returnStatus;

                    // ── 4. FINALIZATION ─────────────────────────────────────────
                    // Export the physical text file receipt and clean up the UI collection
                    CarDataService.GenerateReturnReport(SelectedRental);
                    ActiveRentals.Remove(SelectedRental);

                    MessageBox.Show($"Car Returned Successfully!\nFinal Revenue from this rental: ${finalAmount:F2}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Return failed: {ex.Message}");
                }
            });

            LoadActiveRentals();
        }

        /// <summary>
        /// Initially populates the list by fetching all rentals and filtering for 'Active' status.
        /// </summary>
        private async void LoadActiveRentals()
        {
            var all = await CarDataService.GetAllRentals();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ActiveRentals.Clear();
                foreach (var r in all)
                {
                    if (r.Status == "Active")
                        ActiveRentals.Add(r);
                }
            });
        }
    }
}