using ReactiveUI;
using SQLite;
using System;

namespace AvaloniaChat.Models
{
    [Table("ChatMessages")]
    public class ChatMessage : ReactiveObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Indexed]
        public int SessionId { get; set; }
        
        public string SenderLabel { get; set; }
        
        private string content;
        public string Content
        {
            get => content;
            set=> this.RaiseAndSetIfChanged(ref content, value);
        }

        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public int Order { get; set; }
        
        [Ignore]
        public Sender Sender { get; set; }
        
        [Ignore]
        public bool IsUser { get; set; }
        
        public ChatMessage()
        {
            // 必须提供无参构造函数以便SQLite
        }
        
        public ChatMessage(Sender sender, string content)
        {
            Sender = sender;
            SenderLabel = sender.Label;
            Content = content;
            IsUser = sender.Label == "用户";
        }
    }
} 