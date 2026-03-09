using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class StubWindowViewModel : ObservableObject
    {
        private readonly string _userId;
        private readonly bool _isAdmin;

        private string _userLabel = string.Empty;
        public string UserLabel
        {
            get => _userLabel;
            set { _userLabel = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }

        public StubWindowViewModel(string userId, bool isAdmin)
        {
            _userId = userId;
            _isAdmin = isAdmin;
            UserLabel = isAdmin ? $"Agent: {userId}" : $"Customer: {userId}";

            BackCommand = new RelayCommand(_ =>
            {
                if (_isAdmin)
                    NavigationService.Navigate(new View.AdminDashboard(_userId));
                else
                    NavigationService.Navigate(new View.CustomerDashboard(_userId));
            });
        }
    }
}