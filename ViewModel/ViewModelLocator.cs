using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using QueryToJson.Model;

namespace QueryToJson.ViewModel
{
    public class ViewModelLocator
    {
        public MainWindowVM MainVM => ServiceLocator.Current.GetInstance<MainWindowVM>();
        public LogVM LogVM => ServiceLocator.Current.GetInstance<LogVM>();
        public ServersVM ServersVM => ServiceLocator.Current.GetInstance<ServersVM>();
        public FunctionsVM FunctionsVM => ServiceLocator.Current.GetInstance<FunctionsVM>();

        private XMLHandler xmlHandler = XMLHandler.Instance;

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainWindowVM>();
            SimpleIoc.Default.Register<LogVM>();
            SimpleIoc.Default.Register(() => new ServersVM(xmlHandler.LoadXmlFile("Servers.xml")));
            SimpleIoc.Default.Register(() => new FunctionsVM(xmlHandler.LoadXmlFile("ApiFunctions.xml")));
        }
    }

    public class ShowMessageBar : MessageBase
    {
        private MessageType _messageType;
        public MessageType MessageType { get => _messageType; set => _messageType = value; }

        public ShowMessageBar(MessageType messageType) => MessageType = messageType;
    }
}