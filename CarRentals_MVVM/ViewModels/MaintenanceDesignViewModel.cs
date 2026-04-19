using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class MaintenanceDesignViewModel
    {
        public string UserLabel { get; } = "Agent: A001";
        public string SelectedCarId { get; set; } = "C001";
        public string TechnicianName { get; set; } = "Juan dela Cruz";
        public string Description { get; set; } = "Oil change and tire rotation";
        public decimal CompletionCost { get; set; } = 500;

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

        public ObservableCollection<CarModel> CarList { get; } = new ObservableCollection<CarModel>
        {
            new CarModel { CarId = "C001", Name = "Toyota Camry",   Status = "Available" },
            new CarModel { CarId = "C006", Name = "Hyundai Tucson", Status = "Maintenance" }
        };
    }
}