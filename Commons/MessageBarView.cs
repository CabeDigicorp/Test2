using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Commons
{
    public class MessageBarView : NotificationBase
    {
        bool _isVisible = false;
        public bool IsVisible { get => _isVisible; protected set => SetProperty(ref _isVisible, value); }

        string _text = "";
        public string Text { get => _text; protected set => SetProperty(ref _text, value); }

        int _progressValue = 0;
        public int ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }

        bool _isProgressBarVisible = true;
        public bool IsProgressBarVisible { get => _isProgressBarVisible; set => SetProperty(ref _isProgressBarVisible, value); }

        public event EventHandler Reply;

        public void Show(string msg, bool isOkButtonVisible = true, int progressValue = -1)
        {
            IsYesButtonVisible = false;
            IsNoButtonVisible = false;
            IsOkButtonVisible = isOkButtonVisible;
            Text = msg;
            IsVisible = true;
            ProgressValue = progressValue;
            if (progressValue >= 0)
                IsProgressBarVisible = true;
            else
                IsProgressBarVisible = false;
        }

        public void ShowQuestion(string msg)
        {
            IsYesButtonVisible = true;
            IsNoButtonVisible = true;
            IsOkButtonVisible = false;
            Text = msg;
            IsVisible = true;
            ProgressValue = -1;
            IsProgressBarVisible = false;

        }


        bool _isYesButtonVisible = false;
        public bool IsYesButtonVisible { get => _isYesButtonVisible; set => SetProperty(ref _isYesButtonVisible, value); }

        bool _isNoButtonVisible = false;
        public bool IsNoButtonVisible { get => _isNoButtonVisible; set => SetProperty(ref _isNoButtonVisible, value); }

        bool _isOkButtonVisible = true;
        public bool IsOkButtonVisible { get => _isOkButtonVisible; set => SetProperty(ref _isOkButtonVisible, value); }

        public ICommand OkCommand
        {
            get
            {
                return new CommandHandler(() => this.Ok());
            }
        }

        public void Ok()
        {
            Text = "";
            IsVisible = false;
            ProgressValue = 0;

            OnReply(new MessageBarReplyEventArgs() { Confirmed = true });
            
        }

        protected virtual void OnReply(EventArgs e)
        {
            Reply?.Invoke(this, e);
            Reply = null;//scopo: per far in modo che l'evento parta una volta sola
        }

        public ICommand YesCommand
        {
            get
            {
                return new CommandHandler(() => this.Confirm());
            }
        }

        public void Confirm()
        {
            Text = "";
            IsVisible = false;
            ProgressValue = 0;

            OnReply(new MessageBarReplyEventArgs() { Confirmed = true });
        }

        public ICommand NoCommand
        {
            get
            {
                return new CommandHandler(() => this.NoConfirm());
            }
        }

        public void NoConfirm()
        {
            Text = "";
            IsVisible = false;
            ProgressValue = 0;

            OnReply(new MessageBarReplyEventArgs() { Confirmed = false });
        }

    }

    public class MessageBarReplyEventArgs : EventArgs
    {
        public bool Confirmed { get; set; } = false;
    }

}
