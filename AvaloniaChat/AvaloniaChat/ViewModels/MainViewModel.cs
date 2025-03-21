using AvaloniaChat.Models;
using AvaloniaChat.Services;
using Avalonia.Threading;
using Microsoft.SemanticKernel;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AvaloniaChat.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ChatMessage> Messages { get; set; } = [];
        private readonly IChatService _chatService;
        private readonly DatabaseService _databaseService;
        private readonly DispatcherTimer _animationTimer;
        private string _modelInfo = string.Empty;
        private int _currentOrder = 0;

        private int _currentSessionId;
        public int CurrentSessionId
        {
            get => _currentSessionId;
            set => this.RaiseAndSetIfChanged(ref _currentSessionId, value);
        }

        private string _sessionTitle = "新的对话";
        public string SessionTitle
        {
            get => _sessionTitle;
            set => this.RaiseAndSetIfChanged(ref _sessionTitle, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => this.RaiseAndSetIfChanged(ref _isProcessing, value);
        }

        private double _dotOpacity1 = 0.3;
        public double DotOpacity1
        {
            get => _dotOpacity1;
            set => this.RaiseAndSetIfChanged(ref _dotOpacity1, value);
        }

        private double _dotOpacity2 = 0.3;
        public double DotOpacity2
        {
            get => _dotOpacity2;
            set => this.RaiseAndSetIfChanged(ref _dotOpacity2, value);
        }

        private double _dotOpacity3 = 0.3;
        public double DotOpacity3
        {
            get => _dotOpacity3;
            set => this.RaiseAndSetIfChanged(ref _dotOpacity3, value);
        }

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

        public string ModelInfo
        {
            get => _modelInfo;
            set => this.RaiseAndSetIfChanged(ref _modelInfo, value);
        }

        public ReactiveCommand<Unit,Unit> SendCommand { get; }
        public ReactiveCommand<Unit,Unit> NewSessionCommand { get; }
        public ReactiveCommand<Unit,Unit> BackToSessionsCommand { get; }
        
        // 导航事件
        public event EventHandler BackToSessionsRequested;

        public MainViewModel()
        {
            // 初始化数据库服务
            _databaseService = new DatabaseService();

            // 初始化动画计时器
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(350)
            };
            _animationTimer.Tick += AnimationTimer_Tick;

            // 初始化Semantic Kernel
            var builder = Kernel.CreateBuilder();

            var config = ConfigService.ListAsync().GetAwaiter().GetResult();

            string apiKey = config.Single(x => x.Key == "ApiKey").Value;
            string apiUri = config.Single(x => x.Key == "Endpoint").Value;
            string modelId = config.Single(x => x.Key == "ModelId").Value;
            _chatService = new SemanticKernelChatService(apiKey, apiUri, modelId);
            
            // 设置模型信息
            ModelInfo = $"模型: {modelId}";

            // 创建新会话
            CreateNewSession();

            // 配置命令
            SendCommand = ReactiveCommand.CreateFromTask(SendMessage);
            NewSessionCommand = ReactiveCommand.Create(CreateNewSession);
            BackToSessionsCommand = ReactiveCommand.Create(() => 
            {
                BackToSessionsRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task LoadSessionAsync(int sessionId)
        {
            // 清空当前消息和聊天历史
            Messages.Clear();
            _chatService.ClearHistory();
            _currentOrder = 0;

            // 加载会话
            var session = await _databaseService.GetSessionAsync(sessionId);
            CurrentSessionId = sessionId;
            SessionTitle = session.Title;

            // 添加会话消息
            foreach (var message in session.Messages)
            {
                Messages.Add(message);
                
                // 更新聊天历史
                if (message.SenderLabel == Sender.User.Label)
                {
                    _chatService.AddUserMessage(message.Content);
                }
                else if (message.SenderLabel == Sender.Assistant.Label)
                {
                    _chatService.AddAssistantMessage(message.Content);
                }

                _currentOrder = Math.Max(_currentOrder, message.Order + 1);
            }
        }

        public void CreateNewSession()
        {
            // 创建新会话
            Messages.Clear();
            _chatService.ClearHistory();
            CurrentSessionId = 0;
            SessionTitle = "新的对话";
            _currentOrder = 0;

            // 添加欢迎消息
            var welcomeMessage = new ChatMessage(Sender.Assistant, $"你好！我是基于AI的助手。有什么我可以帮助你的吗？")
            {
                Order = _currentOrder++
            };
            Messages.Add(welcomeMessage);
        }

        public async Task SaveSessionAsync()
        {
            if (Messages.Count <= 1)
                return; // 只有欢迎消息，不保存

            // 准备会话数据
            var session = new ChatSession
            {
                Id = CurrentSessionId,
                Title = string.IsNullOrEmpty(SessionTitle) || SessionTitle == "新的对话" 
                    ? $"对话 {DateTime.Now:yyyy-MM-dd HH:mm}" 
                    : SessionTitle,
                LastUpdatedAt = DateTime.Now
            };

            // 保存会话
            var sessionId = await _databaseService.SaveSessionAsync(session);
            CurrentSessionId = sessionId;

            // 如果是新会话，保存所有消息
            if (session.Id == 0)
            {
                // 设置消息的会话ID
                foreach (var message in Messages)
                {
                    message.SessionId = sessionId;
                }
                
                await _databaseService.SaveMessagesAsync(Messages, sessionId);
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            UpdateDotOpacities();
        }

        private void UpdateDotOpacities()
        {
            // 循环动画效果
            if (DotOpacity1 > 0.8)
            {
                DotOpacity1 = 0.3;
                DotOpacity2 = 0.8;
                DotOpacity3 = 0.5;
            }
            else if (DotOpacity2 > 0.8)
            {
                DotOpacity1 = 0.5;
                DotOpacity2 = 0.3;
                DotOpacity3 = 0.8;
            }
            else
            {
                DotOpacity1 = 0.8;
                DotOpacity2 = 0.5;
                DotOpacity3 = 0.3;
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput))
                return;

            // 添加用户消息到聊天记录
            var userMessage = new ChatMessage(Sender.User, MessageInput)
            {
                SessionId = CurrentSessionId,
                Order = _currentOrder++
            };
            Messages.Add(userMessage);
            
            SendBtnEnabled = false;
            IsProcessing = true;
            _animationTimer.Start();

            try
            {
                // 添加AI回复到聊天记录
                var assistantMessage = new ChatMessage(Sender.Assistant, string.Empty)
                {
                    SessionId = CurrentSessionId,
                    Order = _currentOrder++
                };
                Messages.Add(assistantMessage);

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

                // 尝试从第一条消息提取标题
                if (CurrentSessionId == 0 && Messages.Count >= 3 && string.IsNullOrEmpty(SessionTitle) || SessionTitle == "新的对话")
                {
                    var firstUserMessage = Messages.FirstOrDefault(m => m.SenderLabel == Sender.User.Label)?.Content;
                    if (!string.IsNullOrEmpty(firstUserMessage))
                    {
                        // 提取标题 (最多20个字符)
                        SessionTitle = firstUserMessage.Length <= 20 
                            ? firstUserMessage 
                            : firstUserMessage.Substring(0, 17) + "...";
                    }
                }

                // 保存会话和消息
                await SaveSessionAsync();

                // 如果是新消息，保存到数据库
                if (userMessage.Id == 0)
                {
                    await _databaseService.SaveMessageAsync(userMessage);
                }
                if (assistantMessage.Id == 0)
                {
                    await _databaseService.SaveMessageAsync(assistantMessage);
                }
            }
            catch (Exception ex)
            {
                // 处理错误
                var errorMessage = new ChatMessage(Sender.System, $"发生错误: {ex.Message}")
                {
                    SessionId = CurrentSessionId,
                    Order = _currentOrder++
                };
                Messages.Add(errorMessage);
                await _databaseService.SaveMessageAsync(errorMessage);
            }
            finally
            {
                SendBtnEnabled = true;
                IsProcessing = false;
                _animationTimer.Stop();
            }
        }
    }
}
