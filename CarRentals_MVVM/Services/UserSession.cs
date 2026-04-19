namespace CarRentals_MVVM.Services
{
    public static class UserSession
    {
        public static string UserId { get; set; } = string.Empty;
        public static string Username { get; set; } = string.Empty;
        public static string FullName { get; set; } = string.Empty;
        public static string Role { get; set; } = string.Empty;

        public static void Clear()
        {
            UserId = string.Empty;
            Username = string.Empty;
            FullName = string.Empty;
            Role = string.Empty;
        }
    }
}