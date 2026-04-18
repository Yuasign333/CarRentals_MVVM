using System;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    public class MaintenanceModel : ObservableObject
    {
        private string _maintenanceId = string.Empty;
        private string _carId = string.Empty;
        private string _technicianName = string.Empty;
        private string _description = string.Empty;
        private DateTime _startDate = DateTime.Now;
        private DateTime? _endDate;
        private decimal _cost = 0;
        private string _status = "In Progress";

        public string MaintenanceId
        {
            get => _maintenanceId;
            set { _maintenanceId = value; OnPropertyChanged(); }
        }
        public string CarId
        {
            get => _carId;
            set { _carId = value; OnPropertyChanged(); }
        }
        public string TechnicianName
        {
            get => _technicianName;
            set { _technicianName = value; OnPropertyChanged(); }
        }
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }
        public decimal Cost
        {
            get => _cost;
            set { _cost = value; OnPropertyChanged(); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }
    }
}