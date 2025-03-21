using AvaloniaChat.Models;
using AvaloniaChat.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaloniaChat.ViewModels
{
    public class SessionsViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<ChatSession> _sessions = new();
        
        public ObservableCollection<ChatSession> Sessions 
        { 
            get => _sessions;
            set => this.RaiseAndSetIfChanged(ref _sessions, value);
        }

        public ReactiveCommand<Unit, Unit> CreateNewSessionCommand { get; }
        public ReactiveCommand<int, Unit> OpenSessionCommand { get; }
        public ReactiveCommand<int, Unit> DeleteSessionCommand { get; }

        // 会话选择事件
        public event EventHandler<int> SessionSelected;
        public event EventHandler NewSessionRequested;

        public SessionsViewModel()
        {
            _databaseService = new DatabaseService();

            // 初始化命令
            CreateNewSessionCommand = ReactiveCommand.Create(CreateNewSession);
            OpenSessionCommand = ReactiveCommand.Create<int>(OpenSession);
            DeleteSessionCommand = ReactiveCommand.CreateFromTask<int>(DeleteSession);

            // 加载会话列表
            LoadSessionsAsync().ConfigureAwait(false);
        }

        private async Task LoadSessionsAsync()
        {
            var sessions = await _databaseService.GetSessionsAsync();
            Sessions = new ObservableCollection<ChatSession>(sessions);
        }

        private void CreateNewSession()
        {
            NewSessionRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenSession(int sessionId)
        {
            SessionSelected?.Invoke(this, sessionId);
        }

        private async Task DeleteSession(int sessionId)
        {
            await _databaseService.DeleteSessionAsync(sessionId);
            await LoadSessionsAsync();
        }

        public void RefreshSessions()
        {
            LoadSessionsAsync().ConfigureAwait(false);
        }
    }
} 