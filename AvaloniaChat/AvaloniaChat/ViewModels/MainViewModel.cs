using AvaloniaChat.Models;
using AvaloniaChat.Services;
using Microsoft.SemanticKernel;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaChat.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ChatMessage> Messages { get; set; } = [];
        private readonly IChatService _chatService;

        private bool _sendBtnEnabled = true;

        public bool SendBtnEnabled
        { 
            get=>_sendBtnEnabled;
            set=>this.RaiseAndSetIfChanged(ref _sendBtnEnabled,value); 
        }

        private string _messageInput = string.Empty;
        public string MessageInput
        { 
            get=>_messageInput; 
            set => this.RaiseAndSetIfChanged(ref _messageInput,value);
        }

        public ReactiveCommand<Unit,Unit> SendCommand { get; }
        public MainViewModel()
        {
            // 设置ItemsControl的数据源

            // 初始化Semantic Kernel
            var builder = Kernel.CreateBuilder();

            var config = ConfigService.ListAsync().GetAwaiter().GetResult();

            string apiKey = config.Single(x => x.Key == "ApiKey").Value;
            string apiUri = config.Single(x => x.Key == "Endpoint").Value;
            string modelId = config.Single(x => x.Key == "ModelId").Value;
            _chatService = new SemanticKernelChatService(apiKey, apiUri, modelId);

            // 添加欢迎消息
            Messages.Add(new ChatMessage(Sender.Assistant, $"你好！我是基于{modelId}的AI助手。有什么我可以帮助你的吗？"));

            SendCommand = ReactiveCommand.CreateFromTask(SendMessage);
        }

        private async Task SendMessage()
        {

            if (string.IsNullOrEmpty(MessageInput))
                return;

            // 添加用户消息到聊天记录
            Messages.Add(new ChatMessage(Sender.User, MessageInput));
            SendBtnEnabled = false;
            try
            {
                // 添加AI回复到聊天记录
                Messages.Add(new ChatMessage(Sender.Assistant, string.Empty));

                // 使用Semantic Kernel获取AI回复
                var stream = _chatService.GetStreamResponse(MessageInput);
                StringBuilder sb = new StringBuilder();

                // 清空输入框
                MessageInput = string.Empty;

                await foreach (var chunk in stream)
                {
                    Messages[^1].Content += chunk;
                    sb.Append(chunk);
                }
                _chatService.AddAssistantMessage(sb.ToString());
            }
            catch (Exception ex)
            {
                // 处理错误
                Messages.Add(new ChatMessage(Sender.System, $"发生错误: {ex.Message}"));
            }
            finally
            {
                SendBtnEnabled= true;
            }
        }
    }
}
