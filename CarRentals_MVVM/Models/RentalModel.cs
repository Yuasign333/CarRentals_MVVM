using System;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    public class RentalModel : ObservableObject
    {
        private string _rentalId = string.Empty;
        private string _customerId = string.Empty;
        private string _carId = string.Empty;
        private string _carName = string.Empty;
        private string _driverName = string.Empty;
        private string _color = string.Empty;
        private DateTime _startDate = DateTime.Today;
        private int _hours = 1;
        private decimal _basePrice;
        private decimal _deposit = 50;
        private string _status = "Active";

        public string RentalId
        {
            get => _rentalId;
            set { if (_rentalId != value) { _rentalId = value; OnPropertyChanged(); } }
        }

        public string CustomerId
        {
            get => _customerId;
            set { if (_customerId != value) { _customerId = value; OnPropertyChanged(); } }
        }

        public string CarId
        {
            get => _carId;
            set { if (_carId != value) { _carId = value; OnPropertyChanged(); } }
        }

        public string CarName
        {
            get => _carName;
            set { if (_carName != value) { _carName = value; OnPropertyChanged(); } }
        }

        public string DriverName
        {
            get => _driverName;
            set { if (_driverName != value) { _driverName = value; OnPropertyChanged(); } }
        }

        public string Color
        {
            get => _color;
            set { if (_color != value) { _color = value; OnPropertyChanged(); } }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { if (_startDate != value) { _startDate = value; OnPropertyChanged(); } }
        }

        public int Hours
        {
            get => _hours;
            set
            {
                if (_hours != value)
                {
                    _hours = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                if (_basePrice != value)
                {
                    _basePrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        public decimal Deposit
        {
            get => _deposit;
            set
            {
                if (_deposit != value)
                {
                    _deposit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        public decimal TotalCost => BasePrice + Deposit;

        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }
    }
}