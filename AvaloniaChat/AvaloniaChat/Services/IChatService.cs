using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaChat.Services
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(string message);
        IAsyncEnumerable<StreamingKernelContent> GetStreamResponse(string message);
        void AddAssistantMessage(string message);
    }
} 