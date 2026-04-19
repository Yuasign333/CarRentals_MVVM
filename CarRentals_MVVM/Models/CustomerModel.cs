using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    public class CustomerModel : ObservableObject
    {
        private string _customerId = string.Empty;
        private string _fullName = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _contactNumber = string.Empty;
        private string _licenseNumber = string.Empty;
        private string _securityQuestion = string.Empty;
        private string _securityAnswer = string.Empty;
        private string _profilePicturePath = string.Empty;
     

        public string CustomerId
        {
            get => _customerId;
            set { _customerId = value; OnPropertyChanged(); }
        }
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }
        public string ContactNumber
        {
            get => _contactNumber;
            set { _contactNumber = value; OnPropertyChanged(); }
        }
        public string LicenseNumber
        {
            get => _licenseNumber;
            set { _licenseNumber = value; OnPropertyChanged(); }
        }

        public string SecurityQuestion
        {
            get => _securityQuestion;
            set { _securityQuestion = value; OnPropertyChanged(); }
        }
        public string SecurityAnswer
        {
            get => _securityAnswer;
            set { _securityAnswer = value; OnPropertyChanged(); }
        }

        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set { _profilePicturePath = value; OnPropertyChanged(); }
        }
    }
}