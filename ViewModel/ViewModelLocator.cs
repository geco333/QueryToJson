/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:QueryToJson"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace QueryToJson.ViewModel
{
    public class ViewModelLocator
    {
        private XMLHandler xmlHandler = XMLHandler.Instance;

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainWindowVM>();
            SimpleIoc.Default.Register<LogVM>();
            SimpleIoc.Default.Register(() => new ServersVM(xmlHandler));
            SimpleIoc.Default.Register<FunctionsVM>();
            SimpleIoc.Default.Register<ParametersVM>();

            //Messenger.Default.Register<NotificationMessage>(this, NotifyUserMethod);
        }

        public MainWindowVM MainVM => ServiceLocator.Current.GetInstance<MainWindowVM>();
        public LogVM LogVM => ServiceLocator.Current.GetInstance<LogVM>();
        public ServersVM ServersVM => ServiceLocator.Current.GetInstance<ServersVM>();
        public FunctionsVM FunctionsVM => ServiceLocator.Current.GetInstance<FunctionsVM>();
        public ParametersVM ParametersVM => ServiceLocator.Current.GetInstance<ParametersVM>();

        //private void NotifyUserMethod(NotificationMessage message) { }
    }

    public class ShowMessage : MessageBase
    {
        private MessageType _messageType;
        public MessageType MessageType { get => _messageType; set => _messageType = value; }

        public ShowMessage(MessageType messageType) => MessageType = messageType;
    }
}