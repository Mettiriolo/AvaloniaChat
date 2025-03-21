using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaChat.Services
{
    public class SemanticKernelChatService : IChatService
    {
        private readonly Kernel _kernel;
        private readonly ChatHistory _chatHistory = new();
        private readonly IChatCompletionService _chatCompletionService;

        public SemanticKernelChatService(string apiKey, string apiUri, string modelId)
        {
            var builder = Kernel.CreateBuilder();

            OpenAIClientOptions options = new()
            {
                Endpoint = new Uri(apiUri)
            };

            // 创建 OpenAI 凭证
            ApiKeyCredential credential = new(apiKey);

            // 创建 OpenAI 客户端
            OpenAIClient client = new(credential, options);

            // 注册 OpenAI 聊天补全服务
            builder.Services.AddOpenAIChatCompletion(
                modelId: modelId,
                openAIClient: client
            );

            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> GetResponseAsync(string message)
        {
            var response = await _kernel.InvokePromptAsync(message);
            return response.ToString();
        }

        public async IAsyncEnumerable<string> GetStreamResponse(string message)
        {
            _chatHistory.Add(new ChatMessageContent(AuthorRole.User, message));
            var stream = _chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory);
            
            await foreach (var content in stream)
            {
                if (content.Content != null)
                {
                    yield return content.Content;
                }
            }
        }

        public void AddUserMessage(string message)
        {
            _chatHistory.AddUserMessage(message);
        }

        public void AddAssistantMessage(string message)
        { 
            _chatHistory.AddAssistantMessage(message);
        }

        public void ClearHistory()
        {
            _chatHistory.Clear();
        }
    }
} 