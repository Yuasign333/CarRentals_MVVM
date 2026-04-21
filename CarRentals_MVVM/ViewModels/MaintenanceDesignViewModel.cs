using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// Design-time only ViewModel for MaintenanceWindow.xaml.
    /// Provides fake maintenance records and car list so the XAML designer
    /// shows a preview of the maintenance form and table without running the app.
    /// This class is NEVER used at runtime — only in the XAML designer via d:DataContext.
    /// Connected to: MaintenanceWindow.xaml (via d:DataContext).
    /// </summary>
    public class MaintenanceDesignViewModel
    {
        /// <summary>
        /// Fake user label shown in the top-right badge in the designer.
        /// </summary>
        public string UserLabel { get; } = "Agent: A001";

        /// <summary>
        /// Fake selected car ID shown in the form's car dropdown in the designer.
        /// </summary>
        public string SelectedCarId { get; set; } = "C001";

        /// <summary>
        /// Fake technician name shown in the form in the designer.
        /// </summary>
        public string TechnicianName { get; set; } = "Juan dela Cruz";

        /// <summary>
        /// Fake description shown in the form in the designer.
        /// </summary>
        public string Description { get; set; } = "Oil change and tire rotation";

        /// <summary>
        /// Fake completion cost shown in the form in the designer.
        /// </summary>
        public decimal CompletionCost { get; set; } = 500;

        /// <summary>
        /// Fake maintenance records shown in the maintenance table in the designer.
        /// Includes both In Progress and Completed records so both status colors can be previewed.
        /// </summary>
        public ObservableCollection<MaintenanceModel> MaintenanceList { get; } = new ObservableCollection<MaintenanceModel>
        {
            new MaintenanceModel
            {
                MaintenanceId  = "M0001",
                CarId          = "C006",
                TechnicianName = "Juan dela Cruz",
                Description    = "Oil change",
                Status         = "In Progress",
                StartDate      = new DateTime(2026, 4, 10)
            },
            new MaintenanceModel
            {
                MaintenanceId  = "M0002",
                CarId          = "C003",
                TechnicianName = "Pedro Reyes",
                Description    = "Tire rotation",
                Status         = "Completed",
                StartDate      = new DateTime(2026, 4, 5),
                EndDate        = new DateTime(2026, 4, 6),
                Cost           = 750
            }
        };

        /// <summary>
        /// Fake car list shown in the car dropdown in the designer.
        /// </summary>
        public ObservableCollection<CarModel> CarList { get; } = new ObservableCollection<CarModel>
        {
            new CarModel { CarId = "C001", Name = "Toyota Camry",   Status = "Available" },
            new CarModel { CarId = "C006", Name = "Hyundai Tucson", Status = "Maintenance" }
        };
    }
}