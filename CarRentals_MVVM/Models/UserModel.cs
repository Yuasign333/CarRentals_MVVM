using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.Models
{
    /// <summary>
    /// Represents a system user — either a Customer or an Admin.
    /// Used by LoginViewModel to hold credentials during the login process.
    /// Connected to: LoginViewModel (authentication), AdminDashboardViewModel,
    /// CustomerDashboardViewModel (receives UserID after login).
    /// </summary>
    public class UserModel : ObservableObject
    {
        // ── Private backing fields ─────────────────────────────────────────────

        private string _userID = string.Empty;
        private string _password = string.Empty;
        private string _role = string.Empty;
        private string _statusMessage = string.Empty;

        // ── Public properties with change notification ─────────────────────────

        /// <summary>
        /// The user's login ID (e.g. "A001" for Admin, "C001" for Customer).
        /// Bound to the ID TextBox in AdminLogin.xaml and CustomerLogin.xaml.
        /// </summary>
        public string UserID
        {
            get => _userID;
            set
            {
                if (_userID != value)
                {
                    _userID = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The user's password.
        /// Set from the PasswordBox in LoginViewModel.ExecuteLogin().
        /// Not bound directly in XAML since PasswordBox doesn't support binding.
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The role of this user: "Admin" or "Customer".
        /// Passed in from AdminLogin.xaml.cs or CustomerLogin.xaml.cs
        /// and used by LoginViewModel to determine which dashboard to navigate to.
        /// </summary>
        public string Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Optional status message for display purposes.
        /// Reserved for future use (e.g. account status messages).
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}