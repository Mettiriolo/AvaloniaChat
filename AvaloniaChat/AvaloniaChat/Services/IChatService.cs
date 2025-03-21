using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaChat.Services
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(string message);
        IAsyncEnumerable<string> GetStreamResponse(string message);
        void AddUserMessage(string message);
        void AddAssistantMessage(string message);
        void ClearHistory();
    }
} 