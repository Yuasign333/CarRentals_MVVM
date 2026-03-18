using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    public class CarModel : ObservableObject
    {
        private string _carId = string.Empty;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private string _fuelType = string.Empty;
        private string _status = "Available";
        private decimal _pricePerHour;
        private string _imageUrl = string.Empty;
        private string[] _availableColors = [];
        private string _lostItemName = string.Empty;

        public string CarId
        {
            get => _carId;
            set { if (_carId != value) { _carId = value; OnPropertyChanged(); } }
        }

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string Category
        {
            get => _category;
            set { if (_category != value) { _category = value; OnPropertyChanged(); } }
        }

        public string FuelType
        {
            get => _fuelType;
            set { if (_fuelType != value) { _fuelType = value; OnPropertyChanged(); } }
        }

        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        public decimal PricePerHour
        {
            get => _pricePerHour;
            set { if (_pricePerHour != value) { _pricePerHour = value; OnPropertyChanged(); } }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set { if (_imageUrl != value) { _imageUrl = value; OnPropertyChanged(); } }
        }

        public string[] AvailableColors
        {
            get => _availableColors;
            set { if (_availableColors != value) { _availableColors = value; OnPropertyChanged(); } }
        }

      
    }
}