using ReactiveUI;
using System;

namespace AvaloniaChat.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        private readonly MainViewModel _chatViewModel;
        private readonly SessionsViewModel _sessionsViewModel;

        public MainWindowViewModel()
        {
            _chatViewModel = new MainViewModel();
            _sessionsViewModel = new SessionsViewModel();

            // 订阅会话选择事件
            _sessionsViewModel.SessionSelected += OnSessionSelected;
            _sessionsViewModel.NewSessionRequested += OnNewSessionRequested;
            _chatViewModel.BackToSessionsRequested += OnBackToSessionsRequested;

            // 默认显示会话列表视图
            CurrentViewModel = _sessionsViewModel;
        }

        private void OnSessionSelected(object? sender, int sessionId)
        {
            _chatViewModel.LoadSessionAsync(sessionId).ConfigureAwait(false);
            CurrentViewModel = _chatViewModel;
        }

        private void OnNewSessionRequested(object? sender, EventArgs e)
        {
            _chatViewModel.CreateNewSession();
            CurrentViewModel = _chatViewModel;
        }

        private void OnBackToSessionsRequested(object? sender, EventArgs e)
        {
            _sessionsViewModel.RefreshSessions();
            CurrentViewModel = _sessionsViewModel;
        }
    }
} 