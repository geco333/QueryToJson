using GalaSoft.MvvmLight;
using System.Windows;

namespace QueryToJson.Model
{
    public class MessageBar : ObservableObject
    {
        private string _message;
        private Visibility _statusVisable = Visibility.Collapsed;

        public string Message
        {
            get => _message;
            set
            {
                Set("Message", ref _message, value);
                _message = value;
            }
        }
        public Visibility StatusVisbility
        {
            get => _statusVisable;
            set
            {
                Set("StatusVisbility", ref _statusVisable, value);
                _statusVisable = value;
            }
        }
    }
}
