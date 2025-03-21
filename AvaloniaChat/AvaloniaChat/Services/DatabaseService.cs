using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaChat.Models;
using SQLite;

namespace AvaloniaChat.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private static readonly string DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AvaloniaChat",
            "chatdata.db");

        public DatabaseService()
        {
            // 确保目录存在
            var dbFolder = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder!);
            }

            _database = new SQLiteAsyncConnection(DbPath);
            _database.CreateTableAsync<ChatSession>().Wait();
            _database.CreateTableAsync<ChatMessage>().Wait();
        }

        // 会话管理
        public async Task<List<ChatSession>> GetSessionsAsync()
        {
            var sessions = await _database.Table<ChatSession>().OrderByDescending(s => s.LastUpdatedAt).ToListAsync();

            // 获取每个会话的消息计数和最后一条消息
            foreach (var session in sessions)
            {
                var messages = await _database.Table<ChatMessage>()
                    .Where(m => m.SessionId == session.Id)
                    .OrderByDescending(m => m.Order)
                    .Take(1)
                    .ToListAsync();

                session.MessageCount = await _database.Table<ChatMessage>()
                    .Where(m => m.SessionId == session.Id)
                    .CountAsync();

                if (messages.Any())
                {
                    var lastMessage = messages.First();
                    session.LastMessage = TruncateMessage(lastMessage.Content);
                }
            }

            return sessions;
        }

        public async Task<ChatSession> GetSessionAsync(int id)
        {
            var session = await _database.GetAsync<ChatSession>(id);
            session.Messages = await _database.Table<ChatMessage>()
                .Where(m => m.SessionId == id)
                .OrderBy(m => m.Order)
                .ToListAsync();

            // 设置Sender对象，因为SQLite不存储这个复杂对象
            foreach (var message in session.Messages)
            {
                if (message.SenderLabel == Sender.User.Label)
                    message.Sender = Sender.User;
                else if (message.SenderLabel == Sender.Assistant.Label)
                    message.Sender = Sender.Assistant;
                else if (message.SenderLabel == Sender.System.Label)
                    message.Sender = Sender.System;

                message.IsUser = message.SenderLabel == Sender.User.Label;
            }

            return session;
        }

        public async Task<int> SaveSessionAsync(ChatSession session)
        {
            if (session.Id == 0)
            {
                // 新会话
                return await _database.InsertAsync(session);
            }
            else
            {
                // 更新会话
                session.LastUpdatedAt = DateTime.Now;
                await _database.UpdateAsync(session);
                return session.Id;
            }
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            // 删除会话及其所有消息
            await _database.DeleteAsync<ChatSession>(sessionId);
            await _database.ExecuteAsync("DELETE FROM ChatMessages WHERE SessionId = ?", sessionId);
        }

        // 消息管理
        public async Task<int> SaveMessageAsync(ChatMessage message)
        {
            if (message.Id == 0)
            {
                return await _database.InsertAsync(message);
            }
            else
            {
                await _database.UpdateAsync(message);
                return message.Id;
            }
        }

        public async Task SaveMessagesAsync(IEnumerable<ChatMessage> messages, int sessionId)
        {
            foreach (var message in messages)
            {
                message.SessionId = sessionId;
            }
            
            await _database.InsertAllAsync(messages);
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(int sessionId)
        {
            return await _database.Table<ChatMessage>()
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Order)
                .ToListAsync();
        }

        public async Task ClearMessagesAsync(int sessionId)
        {
            await _database.ExecuteAsync("DELETE FROM ChatMessages WHERE SessionId = ?", sessionId);
        }

        // 辅助方法
        private string TruncateMessage(string message, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            
            // 移除Markdown格式
            var plainText = message.Replace("#", "").Replace("*", "").Replace("`", "").Replace("\n", " ");
            
            if (plainText.Length <= maxLength)
                return plainText;
                
            return plainText.Substring(0, maxLength) + "...";
        }
    }
} 