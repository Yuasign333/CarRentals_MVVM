using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    public class UserModel : ObservableObject
    {
        private string _userID = string.Empty;
        private string _password = string.Empty;
        private string _role = string.Empty;
        private string _statusMessage = string.Empty;

        public string UserID
        {
            get => _userID;
            set { if (_userID != value) { _userID = value; OnPropertyChanged(); } }
        }
        public string Password
        {
            get => _password;
            set { if (_password != value) { _password = value; OnPropertyChanged(); } }
        }
        public string Role
        {
            get => _role;
            set 
            { 
                if (_role != value) 
                
                { 
                    _role = value; OnPropertyChanged(); 
                } 
            }
        }
        public string StatusMessage
        {
            get => _statusMessage;

            set 
            { if (_statusMessage != value) 
                { 
                    _statusMessage = value; OnPropertyChanged(); 
                } 
            }
        }
    }
}