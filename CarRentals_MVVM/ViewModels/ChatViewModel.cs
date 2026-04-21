// ─────────────────────────────────────────────────────────────────────────────
// FILE: ChatViewModel.cs
// Connected to: ChatWindow.xaml (View), ChatWindow.xaml.cs (sets DataContext),
//               CarDataService (SaveChatMessage, GetChatMessages, GetCustomerById),
//               AdminChatListWindow (admin navigates back here).
// Purpose: Manages the real-time chat window for both Customer and Admin roles.
//          Customer side: sends messages + receives auto-replies saved to SQL.
//          Admin side: sends messages only (no auto-reply) + loads chat history.
//          Messages are persisted in the ChatMessages SQL table via stored procedures.
//          LoadMessages() runs on init to restore previous conversation history.
// Commands: SendCommand (AsyncRelayCommand), BackCommand (RelayCommand).
// ─────────────────────────────────────────────────────────────────────────────

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
    public class ChatViewModel : ObservableObject
    {
        // The logged-in user's ID (sender)
        private readonly string _myId;
        // The other participant's ID (recipient)
        private readonly string _otherId;
        // "Admin" or "Customer" — controls auto-reply and back navigation
        private readonly string _role;

        /// <summary>Shown in the top-right badge (e.g. "Customer: juandc").</summary>
        public string UserLabel { get; }

        /// <summary>Shown in the chat header (e.g. "Support Chat | Instantly Replies").</summary>
        public string ChatTitle { get; }

        /// <summary>
        /// The live list of chat messages shown in the scrollable message list.
        /// Populated from SQL on init and updated after each Send.
        /// IsFromUser controls left/right bubble alignment.
        /// </summary>
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        private string _currentMessage = string.Empty;
        /// <summary>
        /// Bound to the message input TextBox.
        /// Cleared to empty string after each successful send.
        /// </summary>
        public string CurrentMessage
        {
            get => _currentMessage;
            set { _currentMessage = value; OnPropertyChanged(); }
        }

        // ── Admin-only header properties ───────────────────────────────────────
        // When role is "Admin", these populate the chat header with the customer's
        // name and profile picture (loaded async from DB via LoadCustomerDetails).
        // When role is "Customer", FullName = "Live Agent Support" (hardcoded).

        private string _fullName = string.Empty;
        /// <summary>Customer's full name (admin view) or "Live Agent Support" (customer view).</summary>
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _profilePicturePath = string.Empty;
        /// <summary>
        /// Local file path to the customer's profile picture (admin view only).
        /// Empty string if no profile picture has been set.
        /// </summary>
        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set { _profilePicturePath = value; OnPropertyChanged(); }
        }

        private string _customerId = string.Empty;
        /// <summary>
        /// Subtitle shown under the name in the chat header.
        /// Admin view: "Customer | C001". Customer view: "Online - Replies instantly".
        /// </summary>
        public string CustomerId
        {
            get => _customerId;
            set { _customerId = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Subtitle line shown below the chat title.
        /// Customer view: "Online — Replies instantly".
        /// Admin view: "Chatting with: {customerId}".
        /// </summary>
        public string ChatSubtitle => _role == "Admin"
            ? $"Chatting with: {_otherId}"
            : "Online — Replies instantly";

        public ICommand SendCommand { get; }
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the chat ViewModel for either Customer or Admin role.
        /// </summary>
        /// <param name="myId">The logged-in user's ID (sender).</param>
        /// <param name="otherId">The other participant's ID (recipient).</param>
        /// <param name="role">"Customer" or "Admin" — controls behavior and navigation.</param>
        public ChatViewModel(string myId, string otherId, string role)
        {
            _myId = myId;
            _otherId = otherId;
            _role = role;

            // Badge label uses username from session for customers
            UserLabel = role == "Admin"
                ? $"Agent: {myId}"
                : $"Customer: {UserSession.Username ?? myId}";

            ChatTitle = role == "Admin"
                ? $"Customer Support: {otherId}"
                : "Support Chat | Instantly Replies";

            // Admin: load the customer's name and photo for the header
            // Customer: show generic agent label
            if (role == "Admin")
            {
                CustomerId = $"Customer | {_otherId}";
                LoadCustomerDetails(_otherId);
            }
            else
            {
                FullName = "Live Agent Support";
                CustomerId = "Online - Replies instantly";
            }

            // Back: admin returns to chat inbox, customer returns to dashboard
            BackCommand = new RelayCommand(_ =>
            {
                if (role == "Admin")
                    NavigationService.Navigate(new View.AdminChatListWindow(myId));
                else
                    NavigationService.Navigate(new View.CustomerDashboard(myId));
            });

            // Send: save message to DB, show it, then auto-reply if customer
            SendCommand = new AsyncRelayCommand(async _ =>
            {
                if (string.IsNullOrWhiteSpace(CurrentMessage)) return;

                string text = CurrentMessage.Trim();
                CurrentMessage = string.Empty; // Clear input immediately

                // 1. Persist the user's message to the ChatMessages table
                await CarDataService.SaveChatMessage(_myId, _otherId, text);

                // 2. Add to the UI collection immediately (no reload needed)
                Messages.Add(new ChatMessage
                {
                    SenderId = _myId,
                    ReceiverId = _otherId,
                    Text = text,
                    Time = DateTime.Now.ToString("HH:mm"),
                    IsFromUser = true
                });

                // 3. Auto-reply only triggers for Customer role (not Admin)
                if (_role == "Customer")
                {
                    // Short delay for a natural conversation feel
                    await Task.Delay(500);

                    string replyText = GetAutoReply(text.ToLower());

                    // Save the auto-reply as coming FROM the admin (A001) TO the customer
                    await CarDataService.SaveChatMessage(_otherId, _myId, replyText);

                    Messages.Add(new ChatMessage
                    {
                        SenderId = _otherId,
                        ReceiverId = _myId,
                        Text = replyText,
                        Time = DateTime.Now.ToString("HH:mm"),
                        IsFromUser = false // Left-aligned — from the other party
                    });
                }
            });

            // Load full conversation history from DB on window open
            LoadMessages();
        }

        /// <summary>
        /// Loads the customer's FullName and ProfilePicturePath from the DB
        /// and updates the admin chat header. Runs on a background thread
        /// to avoid blocking the UI during window open.
        /// </summary>
        private void LoadCustomerDetails(string customerId)
        {
            Task.Run(async () =>
            {
                var customer = await CarDataService.GetCustomerById(customerId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (customer != null)
                    {
                        FullName = customer.FullName;
                        ProfilePicturePath = customer.ProfilePicturePath;
                    }
                    else
                    {
                        FullName = "Unknown Customer";
                    }
                });
            });
        }

        /// <summary>
        /// Loads all past messages between the two participants from the DB.
        /// Runs on a background thread — updates Messages collection on UI thread
        /// via Dispatcher.Invoke to avoid cross-thread exceptions.
        /// </summary>
        private void LoadMessages()
        {
            Task.Run(async () =>
            {
                var msgs = await CarDataService.GetChatMessages(_myId, _otherId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var m in msgs)
                    {
                        // Re-evaluate IsFromUser based on the current user's ID
                        m.IsFromUser = m.SenderId == _myId;
                        Messages.Add(m);
                    }
                });
            });
        }

        /// <summary>
        /// Returns a contextual auto-reply based on keywords in the customer's message.
        /// Used only when _role == "Customer". Covers common rental questions:
        /// renting, returning, pricing, cancellation, maintenance, hours, colors, etc.
        /// Falls back to a generic support message for unrecognized input.
        /// </summary>
        private string GetAutoReply(string msg)
        {
            if (msg.Contains("rent") || msg.Contains("book"))
                return "To rent a car, go to Browse Cars from your dashboard and select an available vehicle!";
            if (msg.Contains("return") || msg.Contains("back"))
                return "To return a car, the admin can process your return from the Process Return window.";
            if (msg.Contains("price") || msg.Contains("cost") || msg.Contains("rate"))
                return "Pricing varies per vehicle. Sedans start at $35/hr, SUVs from $55/hr, Vans from $80/hr.";
            if (msg.Contains("cancel"))
                return "To cancel a rental, please contact support directly. Active rentals may have cancellation fees.";
            if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("hey"))
                return "Hello! How can I help you with your rental today?";
            if (msg.Contains("help"))
                return "I can help with: renting cars, pricing, returns, and account questions. What do you need?";
            if (msg.Contains("password") || msg.Contains("forgot"))
                return "To reset your password, go to the login screen and click 'Forgot Password'.";
            if (msg.Contains("maintenance"))
                return "Cars under maintenance are temporarily unavailable. They will return to Available once serviced.";
            if (msg.Contains("hours") || msg.Contains("duration"))
                return "Rental duration is from 1 to 24 hours. Enter your desired hours on the booking form.";
            if (msg.Contains("color"))
                return "Available colors depend on the specific vehicle. You can choose your preferred color during booking!";
            if (msg.Contains("thank"))
                return "You're welcome! Feel free to ask if you need anything else. 😊";

            return "Thank you for your message! Our team will follow up on complex inquiries. For immediate help, please visit your nearest Rental Rev. branch.";
        }
    }
}