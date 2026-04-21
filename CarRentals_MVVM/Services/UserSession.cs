// ─────────────────────────────────────────────────────────────────────────────
// FILE: UserSession.cs
// Connected to: LoginViewModel (writes), CustomerDashboardViewModel (reads),
//               BrowseCarsViewModel (reads), RentCarViewModel (reads),
//               MyAccountViewModel (reads + writes on username change),
//               MyRentalsViewModel (reads), ChatViewModel (reads),
//               LogoutCommand in any ViewModel (calls Clear()).
// Purpose: Static singleton that holds the logged-in user's identity across
//          all windows for the duration of the session.
//          Populated by LoginViewModel.ExecuteLogin() on successful login.
//          Cleared by LogoutCommand (CustomerDashboard and AdminDashboard).
//          Does NOT persist — resets to empty on app restart.
// ─────────────────────────────────────────────────────────────────────────────

namespace CarRentals_MVVM.Services
{
    public static class UserSession
    {
        /// <summary>
        /// The database CustomerID of the logged-in user (e.g. "C001").
        /// Used to query rentals, save rentals, and delete the account.
        /// For admins this is their AdminId (e.g. "A001").
        /// </summary>
        public static string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The login username (from Customers.Username, e.g. "juandc").
        /// Displayed in top-right badges across all customer windows.
        /// Distinct from UserId — customers log in with Username, not CustomerID.
        /// </summary>
        public static string Username { get; set; } = string.Empty;

        /// <summary>
        /// The customer's full name (from Customers.FullName, e.g. "Juan Dela Cruz").
        /// Used in the dashboard WelcomeText ("Welcome back, Juan Dela Cruz!").
        /// </summary>
        public static string FullName { get; set; } = string.Empty;

        /// <summary>
        /// The role of the logged-in user: "Admin" or "Customer".
        /// Used by LoginViewModel to determine which dashboard to navigate to.
        /// </summary>
        public static string Role { get; set; } = string.Empty;

        /// <summary>
        /// Resets all session fields to empty strings.
        /// Called by LogoutCommand before navigating back to ChooseRole.
        /// Ensures no stale user data leaks into the next login session.
        /// </summary>
        public static void Clear()
        {
            UserId = string.Empty;
            Username = string.Empty;
            FullName = string.Empty;
            Role = string.Empty;
        }
    }
}