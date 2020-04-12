using GalaSoft.MvvmLight;
using QueryToJson.Model;
using System;
using System.Windows;
using System.Windows.Threading;

namespace QueryToJson.ViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        private MessageBar _messageBar = new MessageBar();
        public MessageBar MessageBar { get => _messageBar; set => _messageBar = value; }

        public MainWindowVM() => MessengerInstance.Register<ShowMessage>(this, ShowMessage);

        public void ShowMessage(ShowMessage showMessage)
        {
            switch (showMessage.MessageType)
            {
                case MessageType.AddServer:
                    MessageBar.Message = "Server added.";
                    break;

                case MessageType.AddServerError:
                    MessageBar.Message = "Error: server name and ip already exists.";
                    break;

                case MessageType.DelServer:
                    MessageBar.Message = "Server deleted.";
                    break;

                default:
                    break;
            }

            ToggleMessageBar();
        }

        private void ToggleMessageBar()
        {
            MessageBar.StatusVisbility = Visibility.Visible;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (object sender, EventArgs e) =>
            {
                MessageBar.StatusVisbility = Visibility.Collapsed;
                (sender as DispatcherTimer).Stop();
            };

            timer.Start();
        }
    }
}
