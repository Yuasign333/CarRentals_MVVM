// ─────────────────────────────────────────────────────────────────────────────
// Design-time only ViewModel for AdminChatListWindow.xaml.
// Provides fake customer list so the XAML designer shows a preview.
// NEVER used at runtime — only in the XAML designer via d:DataContext.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class AdminChatListDesignViewModel
    {
        /// <summary>
        /// Fake user label shown in the top-right badge in the designer.
        /// </summary>
        public string UserLabel { get; } = "Agent: A001";

        /// <summary>
        /// Fake customer list shown in the chat list in the designer.
        /// </summary>
        public ObservableCollection<CustomerModel> Customers { get; } = new()
        {
            new CustomerModel
            {
                CustomerId  = "C001",
                FullName    = "Juan Dela Cruz",
                Username    = "juandc",
                ProfilePicturePath = ""
            },
            new CustomerModel
            {
                CustomerId  = "C002",
                FullName    = "Maria Santos",
                Username    = "mariasantos",
                ProfilePicturePath = ""
            },
            new CustomerModel
            {
                CustomerId  = "C003",
                FullName    = "Pedro Reyes",
                Username    = "pedroreyes",
                ProfilePicturePath = ""
            }
        };
    }
}