using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for MyAccountWindow.xaml.
    /// Displays the logged-in customer's profile information and rental history.
    /// Allows the customer to:
    ///   - Change their profile picture (saved immediately to DB)
    ///   - Edit their username inline (saved immediately to DB)
    ///   - Delete their account permanently
    /// Connected to: MyAccountWindow.xaml (View),
    /// MyAccountWindow.xaml.cs (sets DataContext),
    /// CarDataService (reads profile + rentals, updates profile, deletes account),
    /// UserSession (reads and updates the active session data),
    /// CustomerDashboard (navigates back here on Back),
    /// BrowseCarsWindow (navigates here on Browse Cars).
    /// </summary>
    public class MyAccountViewModel : ObservableObject
    {
        // The logged-in customer's user ID
        private readonly string _userId;

        // ── Display properties ─────────────────────────────────────────────────

        private string _userLabel = string.Empty;

        /// <summary>
        /// Label shown in the top-right badge (e.g. "Customer: juandc").
        /// Updates when the username is changed.
        /// </summary>
        public string UserLabel
        {
            get => _userLabel;
            set
            {
                _userLabel = value;
                OnPropertyChanged();
            }
        }

        private string _username = string.Empty;

        /// <summary>
        /// The customer's current username.
        /// Displayed in the profile card and used as the user label.
        /// Updates after SaveUsernameCommand succeeds.
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The customer's full name loaded from the database.
        /// Read-only — displayed in the profile card.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// The customer's ID (e.g. "C001").
        /// Read-only — displayed in the profile card.
        /// </summary>
        public string CustomerId { get; }

        private string _contact = "Loading...";

        /// <summary>
        /// The customer's contact number loaded from the database.
        /// Displayed in the profile card.
        /// </summary>
        public string Contact
        {
            get => _contact;
            set
            {
                _contact = value;
                OnPropertyChanged();
            }
        }

        private string _license = "Loading...";

        /// <summary>
        /// The customer's license number loaded from the database.
        /// Displayed in the profile card.
        /// </summary>
        public string License
        {
            get => _license;
            set
            {
                _license = value;
                OnPropertyChanged();
            }
        }

        // ── Username editing ───────────────────────────────────────────────────

        private string _newUsername = string.Empty;

        /// <summary>
        /// The new username typed in the inline edit TextBox.
        /// Pre-filled with the current username when editing starts.
        /// Bound to the username edit TextBox in MyAccountWindow.xaml.
        /// </summary>
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                _newUsername = value;
                OnPropertyChanged();
            }
        }

        private bool _isEditingUsername;

        /// <summary>
        /// Controls whether the username edit TextBox is visible.
        /// True when the customer clicks Edit, False after Save or Cancel.
        /// Bound to the Visibility of the edit section in MyAccountWindow.xaml.
        /// </summary>
        public bool IsEditingUsername
        {
            get => _isEditingUsername;
            set
            {
                _isEditingUsername = value;
                OnPropertyChanged();
            }
        }

        // ── Profile picture ────────────────────────────────────────────────────

        private string _profilePicturePath = string.Empty;

        /// <summary>
        /// The file path of the customer's profile picture.
        /// When set, hides the default icon and shows the actual image.
        /// Also notifies HasProfilePicture so the XAML switches between icon and image.
        /// </summary>
        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set
            {
                _profilePicturePath = value;
                OnPropertyChanged();

                // Notify HasProfilePicture so the XAML hides the icon and shows the image
                OnPropertyChanged(nameof(HasProfilePicture));
            }
        }

        /// <summary>
        /// True when the customer has a profile picture set.
        /// Used in XAML to toggle between the default emoji icon and the actual image.
        /// </summary>
        public bool HasProfilePicture => !string.IsNullOrEmpty(_profilePicturePath);

        // ── Error display ──────────────────────────────────────────────────────

        private string _errorMessage = string.Empty;

        /// <summary>
        /// The error text shown when a username update fails.
        /// Bound to the error TextBlock in MyAccountWindow.xaml.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _errorVisible = false;

        /// <summary>
        /// Controls whether the error banner is visible.
        /// Bound to the error Border's Visibility in MyAccountWindow.xaml.
        /// </summary>
        public bool ErrorVisible
        {
            get => _errorVisible;
            set
            {
                _errorVisible = value;
                OnPropertyChanged();
            }
        }

        // ── Rental history ─────────────────────────────────────────────────────

        /// <summary>
        /// The customer's rental history loaded from the database.
        /// Bound to the rentals list in MyAccountWindow.xaml.
        /// </summary>
        public ObservableCollection<RentalModel> Rentals { get; } = new();

        private bool _hasRentals = false;

        /// <summary>
        /// True when the customer has at least one rental.
        /// Used to toggle between the empty-state message and the rentals list.
        /// </summary>
        public bool HasRentals
        {
            get => _hasRentals;
            set
            {
                _hasRentals = value;
                OnPropertyChanged();
            }
        }

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>Opens the file picker to select a new profile picture.</summary>
        public ICommand PickPictureCommand { get; }

        /// <summary>Shows the inline username edit TextBox pre-filled with current username.</summary>
        public ICommand EditUsernameCommand { get; }

        /// <summary>Validates and saves the new username to the database.</summary>
        public ICommand SaveUsernameCommand { get; }

        /// <summary>Cancels username editing and hides the edit TextBox.</summary>
        public ICommand CancelEditUsernameCommand { get; }

        /// <summary>Navigates back to CustomerDashboard.</summary>
        public ICommand BackCommand { get; }

        /// <summary>Navigates to BrowseCarsWindow.</summary>
        public ICommand BrowseCarsCommand { get; }

        /// <summary>Permanently deletes the customer's account after double confirmation.</summary>
        public ICommand DeleteAccountCommand { get; }

        /// <summary>
        /// Initializes the My Account ViewModel for the given customer.
        /// </summary>
        /// <param name="userId">The logged-in customer's user ID.</param>
        public MyAccountViewModel(string userId)
        {
            _userId = userId;
            CustomerId = userId;

            // Use session data for initial display — DB load will sync below
            FullName = !string.IsNullOrEmpty(UserSession.FullName)
                ? UserSession.FullName : userId;
            Username = !string.IsNullOrEmpty(UserSession.Username)
                ? UserSession.Username : userId;
            UserLabel = $"Customer: {Username}";

            // Navigate back to the customer dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.CustomerDashboard(_userId));
            });

            // Navigate to browse cars
            BrowseCarsCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId));
            });

            // Delete account — requires double confirmation before proceeding
            DeleteAccountCommand = new AsyncRelayCommand(async _ =>
            {
                var confirm1 = MessageBox.Show(
                    "Are you sure you want to delete your account?\n\n" +
                    "This will permanently delete all your data and rental history.\n" +
                    "This action cannot be undone.",
                    "Delete Account",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm1 != MessageBoxResult.Yes) return;

                // Second confirmation for safety
                var confirm2 = MessageBox.Show(
                    $"Final confirmation: Delete account for '{Username}'?\n\n" +
                    "This is permanent.",
                    "Are you absolutely sure?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop);

                if (confirm2 != MessageBoxResult.Yes) return;

                try
                {
                    await CarDataService.DeleteAccount(_userId);

                    MessageBox.Show(
                        "Your account has been deleted.\nThank you for using Rental Rev.",
                        "Account Deleted",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Clear session and return to role selection
                    UserSession.Clear();
                    NavigationService.Navigate(new View.ChooseRole());
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    ErrorVisible = true;
                }
            });

            // Load profile data and rentals from the database asynchronously
            _ = LoadUserDataAsync(userId);

            // Change profile picture — opens file picker and saves immediately to DB
            PickPictureCommand = new AsyncRelayCommand(async _ =>
            {
                var result = MessageBox.Show(
                    "Do you want to change your profile picture?",
                    "Change Photo",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No) return;

                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Select Profile Picture"
                };

                if (dialog.ShowDialog() == true)
                {
                    ProfilePicturePath = dialog.FileName;

                    // Save immediately — preserves the current username during the update
                    await CarDataService.UpdateCustomerProfile(
                        _userId, Username, ProfilePicturePath);

                    MessageBox.Show(
                        "Profile picture updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            });

            // Show the inline username edit TextBox
            EditUsernameCommand = new RelayCommand(_ =>
            {
                // Pre-fill the TextBox with the current username
                NewUsername = Username;
                IsEditingUsername = true;
                ErrorVisible = false;
            });

            // Cancel username editing — hide the TextBox without saving
            CancelEditUsernameCommand = new RelayCommand(_ =>
            {
                IsEditingUsername = false;
                ErrorVisible = false;
            });

            // Save the new username to the database
            SaveUsernameCommand = new AsyncRelayCommand(async _ =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to change your username?",
                    "Confirm Change",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No) return;

                ErrorVisible = false;

                // New username is required
                if (string.IsNullOrWhiteSpace(NewUsername))
                {
                    ErrorMessage = "Please enter a new username.";
                    ErrorVisible = true;
                    return;
                }

                // No spaces allowed
                if (NewUsername.Contains(" "))
                {
                    ErrorMessage = "Username cannot contain spaces.";
                    ErrorVisible = true;
                    return;
                }

                // Check if username is already taken by another account
                if (NewUsername != Username &&
                    await CarDataService.UsernameExistsExcept(NewUsername, _userId))
                {
                    ErrorMessage = $"Username '{NewUsername}' is already taken.";
                    ErrorVisible = true;
                    return;
                }

                // Save to database — preserves the current profile picture
                await CarDataService.UpdateCustomerProfile(
                    _userId, NewUsername, ProfilePicturePath);

                // Update local properties and session
                Username = NewUsername;
                UserSession.Username = NewUsername;
                UserLabel = $"Customer: {Username}";
                IsEditingUsername = false;

                MessageBox.Show(
                    "Username updated successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// Loads the customer's profile data and rental history from the database.
        /// Runs on a background thread and updates the UI via Dispatcher.
        /// Called once on initialization instead of using an unawaited Task.Run.
        /// </summary>
        private async Task LoadUserDataAsync(string userId)
        {
            try
            {
                string queryId = !string.IsNullOrEmpty(UserSession.UserId)
                    ? UserSession.UserId : userId;

                var customer = await CarDataService.GetCustomerByUsername(
                    UserSession.Username ?? userId);
                var rentals = await CarDataService.GetRentalsByCustomer(queryId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (customer != null)
                    {
                        Contact = customer.ContactNumber;
                        License = customer.LicenseNumber;
                        ProfilePicturePath = customer.ProfilePicturePath;

                        // Sync UI with the database value in case session got out of sync
                        Username = customer.Username;

                        if (UserSession.Username != customer.Username)
                        {
                            UserSession.Username = customer.Username;
                        }

                        UserLabel = $"Customer: {Username}";
                    }

                    Rentals.Clear();
                    foreach (var r in rentals) Rentals.Add(r);
                    HasRentals = Rentals.Count > 0;
                });
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = "Failed to load account data from the server.";
                    ErrorVisible = true;
                });
            }
        }
    }
}