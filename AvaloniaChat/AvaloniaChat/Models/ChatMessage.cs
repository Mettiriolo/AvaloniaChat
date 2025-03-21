using ReactiveUI;

namespace AvaloniaChat.Models
{
    public class ChatMessage(Sender sender, string content) : ReactiveObject
    {
        public Sender Sender { get; set; } = sender;

        public string Content
        {
            get => content;
            set=> this.RaiseAndSetIfChanged(ref content, value);
        }

        public bool IsUser { get; set; } = sender.Label == "”√ªß";

    }
} 