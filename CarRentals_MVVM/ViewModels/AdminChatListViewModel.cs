using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class AdminChatListViewModel : ObservableObject
    {
        private readonly string _adminId;
        public string UserLabel { get; }
        public ObservableCollection<CustomerModel> Customers { get; } = new();

        private CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand OpenChatCommand { get; }

        public AdminChatListViewModel(string adminId)
        {
            _adminId = adminId;
            UserLabel = $"Agent: {adminId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_adminId)));

            OpenChatCommand = new RelayCommand(_ =>
            {
                if (SelectedCustomer == null) return;
                NavigationService.Navigate(
                    new View.ChatWindow(_adminId, SelectedCustomer.CustomerId, "Admin"));
            });

            // Load customers who have chat history
            Task.Run(async () =>
            {
                var list = await CarDataService.GetChatCustomers();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Customers.Clear();
                    foreach (var c in list) Customers.Add(c);
                });
            });
        }
    }
}