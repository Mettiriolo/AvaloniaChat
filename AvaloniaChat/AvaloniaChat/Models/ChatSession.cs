using SQLite;
using System;
using System.Collections.Generic;

namespace AvaloniaChat.Models
{
    [Table("ChatSessions")]
    public class ChatSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
        
        [Ignore]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        
        [Ignore]
        public int MessageCount { get; set; } = 0;
        
        [Ignore]
        public string LastMessage { get; set; } = string.Empty;
    }
} 