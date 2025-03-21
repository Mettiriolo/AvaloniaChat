using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using AvaloniaChat.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace AvaloniaChat.Views
{
    public partial class MainView : ReactiveUserControl<MainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                // 绑定 ViewModel
                this.Bind(ViewModel, vm => vm.MessageInput, v => v.MessageInput.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.SendCommand, v => v.sendBtn)
                    .DisposeWith(disposables);

                // 监听消息集合变化，自动滚动到底部
                if (ViewModel != null)
                {
                    ((INotifyCollectionChanged)ViewModel.Messages).CollectionChanged += Messages_CollectionChanged;
                }
            });
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 当消息添加或改变时自动滚动到底部
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                ChatScroller.ScrollToEnd();
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                ViewModel?.SendCommand.Execute().Subscribe();
                e.Handled = true;
            }
        }
    }
}