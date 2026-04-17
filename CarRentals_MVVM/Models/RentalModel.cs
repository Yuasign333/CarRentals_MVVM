using System;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    /// <summary>
    /// Represents a single car rental transaction.
    /// Created by BrowseCarsViewModel.ConfirmCommand when a customer confirms a booking.
    /// Stored in CarDataService.Rentals and displayed in MyRentalsWindow.
    /// Connected to: BrowseCarsViewModel (create), MyRentalsViewModel (read),
    /// CarDataService (storage).
    /// </summary>
    public class RentalModel : ObservableObject
    {
        // ── Private backing fields ─────────────────────────────────────────────

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
        private decimal _totalAmount;
        private DateTime _rentalDate = DateTime.Now;


     

        /// <summary>
        /// Unique rental identifier (e.g. "R0001").
        /// Auto-generated in BrowseCarsViewModel.ConfirmCommand.
        /// </summary>
        public string RentalId
        {
            get => _rentalId;
            set
            {
                if (_rentalId != value)
                {
                    _rentalId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The ID of the customer who made this rental (e.g. "C001").
        /// Used by CarDataService.GetByCustomer() to filter rentals per user.
        /// </summary>
        public string CustomerId
        {
            get => _customerId;
            set
            {
                if (_customerId != value)
                {
                    _customerId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The ID of the car being rented (e.g. "C003").
        /// References a CarModel in CarDataService.Cars.
        /// </summary>
        public string CarId
        {
            get => _carId;
            set
            {
                if (_carId != value)
                {
                    _carId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Display name of the rented car (e.g. "Honda Civic").
        /// Stored directly so it displays even if car is later deleted.
        /// </summary>
        public string CarName
        {
            get => _carName;
            set
            {
                if (_carName != value)
                {
                    _carName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the person who will be driving the car.
        /// Entered by the customer on Page 3 of BrowseCarsWindow.
        /// </summary>
        public string DriverName
        {
            get => _driverName;
            set
            {
                if (_driverName != value)
                {
                    _driverName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The color chosen by the customer on Page 2 of BrowseCarsWindow.
        /// </summary>
        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The date this rental officially starts.
        /// Defaults to today's date.
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of hours the car will be rented.
        /// Entered by the customer on Page 3 of BrowseCarsWindow.
        /// Notifies TotalCost when changed.
        /// </summary>
        public int Hours
        {
            get => _hours;
            set
            {
                if (_hours != value)
                {
                    _hours = value;
                    OnPropertyChanged();

                    // Recalculate total when hours change
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        /// <summary>
        /// The base rental cost (PricePerHour x Hours).
        /// Notifies TotalCost when changed.
        /// </summary>
        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                if (_basePrice != value)
                {
                    _basePrice = value;
                    OnPropertyChanged();

                    // Recalculate total when base price changes
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        /// <summary>
        /// The security deposit amount for the rental.
        /// Defaults to 50. Notifies TotalCost when changed.
        /// </summary>
        public decimal Deposit
        {
            get => _deposit;
            set
            {
                if (_deposit != value)
                {
                    _deposit = value;
                    OnPropertyChanged();

                    // Recalculate total when deposit changes
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        /// <summary>
        /// The final total amount paid for this rental.
        /// Set by BrowseCarsViewModel.ConfirmCommand using TotalDue.
        /// </summary>
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The exact date and time the rental was confirmed.
        /// Set automatically by BrowseCarsViewModel.ConfirmCommand.
        /// </summary>
        public DateTime RentalDate
        {
            get => _rentalDate;
            set
            {
                if (_rentalDate != value)
                {
                    _rentalDate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Computed property: BasePrice + Deposit.
        /// Automatically recalculated when BasePrice, Deposit, or Hours change.
        /// </summary>
        public decimal TotalCost => BasePrice + Deposit;

        /// <summary>
        /// Current status of the rental: "Active" by default.
        /// Can be updated by admin in a future Process Return feature.
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}