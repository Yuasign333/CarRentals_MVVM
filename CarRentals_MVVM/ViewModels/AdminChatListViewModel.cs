// ─────────────────────────────────────────────────────────────────────────────
// FILE: AdminChatListViewModel.cs
// Connected to: AdminChatListWindow.xaml (View),
//               AdminChatListWindow.xaml.cs (sets DataContext),
//               CarDataService.GetChatCustomers() (loads inbox),
//               ChatWindow (navigates to when customer is selected).
// Purpose: Admin's chat inbox — shows a list of all customers who have
//          sent at least one message. Admin selects a customer to open
//          the full ChatWindow for that conversation.
//          Customers list is loaded from SQL via sp_GetChatCustomers.
// Commands: BackCommand (RelayCommand), OpenChatCommand (RelayCommand).
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class AdminChatListViewModel : ObservableObject
    {
        // The logged-in admin's ID — passed through to ChatWindow navigation
        private readonly string _adminId;

        /// <summary>Shown in the top-right badge (e.g. "Agent: A001").</summary>
        public string UserLabel { get; }

        /// <summary>
        /// The list of customers who have active chat history.
        /// Populated async from sp_GetChatCustomers on window open.
        /// Bound to the customer list in AdminChatListWindow.xaml.
        /// </summary>
        public ObservableCollection<CustomerModel> Customers { get; } = new();

        private CustomerModel? _selectedCustomer;
        /// <summary>
        /// The customer selected in the inbox list.
        /// OpenChatCommand uses this to navigate to the correct ChatWindow.
        /// Null if no customer is selected yet.
        /// </summary>
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        /// <summary>Navigates back to the AdminDashboard.</summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Opens the full chat conversation with the selected customer.
        /// Passes the admin's ID, the customer's ID, and "Admin" role to ChatWindow.
        /// Does nothing if no customer is selected.
        /// </summary>
        public ICommand OpenChatCommand { get; }

        /// <summary>
        /// Initializes the admin chat inbox for the given admin.
        /// Loads customer list from DB on a background thread.
        /// </summary>
        /// <param name="adminId">The logged-in admin's ID (e.g. "A001").</param>
        public AdminChatListViewModel(string adminId)
        {
            _adminId = adminId;
            UserLabel = $"Agent: {adminId}";

            // Navigate back to the admin main hub
            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_adminId)));

            // Open chat with the selected customer — guard against null selection
            OpenChatCommand = new RelayCommand(_ =>
            {
                if (SelectedCustomer == null) return;
                NavigationService.Navigate(
                    new View.ChatWindow(_adminId, SelectedCustomer.CustomerId, "Admin"));
            });

            // Load the customer inbox list async on init — uses Dispatcher for thread safety
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