using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for ForgotPasswordWindow.xaml.
    /// Handles a 2-step password reset flow:
    ///   Step 1 — Customer enters their username to find their account.
    ///   Step 2 — Customer answers their security question and sets a new password.
    /// Works for both Admin and Customer roles.
    /// Connected to: ForgotPasswordWindow.xaml (View),
    /// ForgotPasswordWindow.xaml.cs (sets DataContext with role),
    /// CarDataService.GetSecurityQuestion() (Step 1 lookup),
    /// CarDataService.ResetPassword() (Step 2 update),
    /// AdminLogin / CustomerLogin (navigates back here on success or Back).
    /// </summary>
    public class ForgotPasswordViewModel : ObservableObject
    {
        // The user's role — determines which login screen to return to
        private readonly string _role;

        // ── Private backing fields ─────────────────────────────────────────────

        private string _username = string.Empty;
        private string _securityQuestion = string.Empty;
        private string _securityAnswer = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _errorVisible = false;
        private bool _isStep2 = false;

        // Stores the CustomerId found in Step 1 — used in Step 2 reset
        private string _customerId = string.Empty;

        // ── Public properties with change notification ─────────────────────────

        /// <summary>
        /// The username entered by the user on Step 1.
        /// Bound to the Username TextBox in ForgotPasswordWindow.xaml.
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
        /// The security question fetched from the database after username is found.
        /// Displayed as the question label on Step 2.
        /// Bound to the SecurityQuestion TextBlock in ForgotPasswordWindow.xaml.
        /// </summary>
        public string SecurityQuestion
        {
            get => _securityQuestion;
            set
            {
                _securityQuestion = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The customer's answer to their security question on Step 2.
        /// Bound to the Security Answer TextBox in ForgotPasswordWindow.xaml.
        /// </summary>
        public string SecurityAnswer
        {
            get => _securityAnswer;
            set
            {
                _securityAnswer = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The new password entered by the customer on Step 2.
        /// Must be at least 6 characters with no spaces.
        /// Bound to the New Password PasswordBox in ForgotPasswordWindow.xaml.
        /// </summary>
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The confirmation of the new password on Step 2.
        /// Must match NewPassword before the reset is allowed.
        /// Bound to the Confirm Password PasswordBox in ForgotPasswordWindow.xaml.
        /// </summary>
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The error text shown when validation fails.
        /// Bound to the error TextBlock in ForgotPasswordWindow.xaml.
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

        /// <summary>
        /// Controls whether the error banner is visible.
        /// Bound to the error Border's Visibility in ForgotPasswordWindow.xaml.
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

        /// <summary>
        /// True when the customer has successfully found their account (Step 2 is active).
        /// Also notifies IsStep1 so both page sections update their Visibility.
        /// Bound to the Visibility of Step 2 content in ForgotPasswordWindow.xaml.
        /// </summary>
        public bool IsStep2
        {
            get => _isStep2;
            set
            {
                _isStep2 = value;
                OnPropertyChanged();

                // Notify IsStep1 so the Step 1 section hides when Step 2 shows
                OnPropertyChanged(nameof(IsStep1));
            }
        }

        /// <summary>
        /// True when Step 1 (username entry) should be visible.
        /// Computed from IsStep2 — Step 1 is visible only when Step 2 is not active.
        /// Bound to the Visibility of Step 1 content in ForgotPasswordWindow.xaml.
        /// </summary>
        public bool IsStep1 => !IsStep2;

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>
        /// Step 1 — Looks up the account by username and retrieves the security question.
        /// On success: shows Step 2. On failure: shows error message.
        /// </summary>
        public ICommand FindAccountCommand { get; }

        /// <summary>
        /// Step 2 — Validates the security answer and new password, then resets in DB.
        /// On success: navigates back to the login screen.
        /// On failure: shows error message.
        /// </summary>
        public ICommand ResetPasswordCommand { get; }

        /// <summary>
        /// Navigates back to the appropriate login screen (Admin or Customer).
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the Forgot Password ViewModel for the given role.
        /// </summary>
        /// <param name="role">"Admin" or "Customer" — determines where Back navigates.</param>
        public ForgotPasswordViewModel(string role)
        {
            _role = role;

            // Navigate back to the correct login screen based on role
            BackCommand = new RelayCommand(_ =>
            {
                if (_role == "Admin")
                {
                    NavigationService.Navigate(new View.AdminLogin());
                }
                else
                {
                    NavigationService.Navigate(new View.CustomerLogin());
                }
            });

            // Step 1 — Find the account by username
            FindAccountCommand = new AsyncRelayCommand(async _ =>
            {
                ErrorVisible = false;

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Please enter your username.";
                    ErrorVisible = true;
                    return;
                }

                // Query the database for the security question linked to this username
                var (found, question, customerId) =
                    await CarDataService.GetSecurityQuestion(Username);

                if (!found)
                {
                    ErrorMessage = "Username not found.";
                    ErrorVisible = true;
                    return;
                }

                // Store the CustomerId for use in Step 2 reset
                _customerId = customerId;
                SecurityQuestion = question;

                // Advance to Step 2 — shows the security question and password fields
                IsStep2 = true;
            });

            // Step 2 — Validate answer and reset the password
            ResetPasswordCommand = new AsyncRelayCommand(async _ =>
            {
                ErrorVisible = false;

                // Security answer is required
                if (string.IsNullOrWhiteSpace(SecurityAnswer))
                {
                    ErrorMessage = "Please enter your security answer.";
                    ErrorVisible = true;
                    return;
                }

                // New password is required
                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    ErrorMessage = "Please enter a new password.";
                    ErrorVisible = true;
                    return;
                }

                // Password minimum length
                if (NewPassword.Length < 6)
                {
                    ErrorMessage = $"Password too short ({NewPassword.Length} chars). Minimum is 6 characters.";
                    ErrorVisible = true;
                    return;
                }

                // No spaces in password
                if (NewPassword.Contains(" "))
                {
                    ErrorMessage = "Password cannot contain spaces.";
                    ErrorVisible = true;
                    return;
                }

                // Confirm password is required
                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    ErrorMessage = "Please confirm your new password.";
                    ErrorVisible = true;
                    return;
                }

                // Passwords must match
                if (NewPassword != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    ErrorVisible = true;
                    return;
                }

                // Call stored procedure sp_ResetPassword — verifies answer and updates DB
                var (success, message) = await CarDataService.ResetPassword(
                    Username, SecurityAnswer, NewPassword);

                if (success)
                {
                    MessageBox.Show(
                        "Password reset successfully! You can now log in.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Return to the appropriate login screen
                    if (_role == "Admin")
                    {
                        NavigationService.Navigate(new View.AdminLogin());
                    }
                    else
                    {
                        NavigationService.Navigate(new View.CustomerLogin());
                    }
                }
                else
                {
                    // sp_ResetPassword returns an error message if answer is wrong
                    ErrorMessage = message;
                    ErrorVisible = true;
                }
            });
        }
    }
}