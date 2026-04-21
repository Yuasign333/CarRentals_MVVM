// ─────────────────────────────────────────────────────────────────────────────
// FILE: ChatMessage.cs
// Connected to: ChatViewModel (creates and stores instances),
//               ChatWindow.xaml (binds Text, Time, IsFromUser, Alignment).
// Purpose: Data model representing a single message in the chat window.
//          IsFromUser controls whether the bubble appears on the right (sender)
//          or left (receiver) side of the chat window via the Alignment property.
//          Alignment is a computed string — WPF HorizontalAlignment accepts
//          "Left" and "Right" as string values via DataBinding.
// ─────────────────────────────────────────────────────────────────────────────

namespace CarRentals_MVVM.Models
{
    public class ChatMessage
    {
        /// <summary>Primary key from the ChatMessages SQL table. Set on load from DB.</summary>
        public int Id { get; set; }

        /// <summary>UserID of the message sender (e.g. "C001" or "A001").</summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>UserID of the message recipient.</summary>
        public string ReceiverId { get; set; } = string.Empty;

        /// <summary>The message content displayed in the chat bubble.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Formatted timestamp string (HH:mm) shown below the message bubble.
        /// Set from SQL SentAt column via DateTime.ToString("HH:mm").
        /// </summary>
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// True if this message was sent by the logged-in user (bubble on right).
        /// False if received from the other party (bubble on left).
        /// Set in GetChatMessages() based on whether SenderId == the current user's ID.
        /// </summary>
        public bool IsFromUser { get; set; }

        /// <summary>
        /// Computed alignment string for XAML HorizontalAlignment binding.
        /// Returns "Right" for sent messages, "Left" for received messages.
        /// </summary>
        public string Alignment => IsFromUser ? "Right" : "Left";
    }
}