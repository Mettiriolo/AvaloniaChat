using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using AvaloniaChat.ViewModels;
using ReactiveUI;
using System;
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
                // °ó¶¨ ViewModel
                this.Bind(ViewModel, vm => vm.MessageInput, v => v.MessageInput.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.SendCommand, v => v.sendBtn)
                    .DisposeWith(disposables);
            });
        }
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel?.SendCommand.Execute().Subscribe();
            }
        }
    }
}