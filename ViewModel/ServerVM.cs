using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using QueryToJson.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace QueryToJson.ViewModel
{
    public enum ServerMVStatus { Free, Add, Edit }

    public class ServersVM : ViewModelBase
    {
        private ObservableCollection<Server> _servers;
        private RelayCommand _deleteServerCommand;
        private RelayCommand _addServerCommand;
        private RelayCommand _toggleAddPanelCommand;
        private Server _selectedServer;
        private Server _newServer;
        private Visibility _addPanelVisibility = Visibility.Collapsed;

        public ObservableCollection<Server> Servers
        {
            get => _servers;
            set => _servers = value;
        }
        public RelayCommand DeleteServerCommand
        {
            get
            {
                if (_deleteServerCommand == null)
                    _deleteServerCommand = new RelayCommand(RemoveServer, true);

                return _deleteServerCommand;
            }
            set { }
        }
        public RelayCommand AddServerCommand
        {
            get
            {
                if (_addServerCommand == null)
                    _addServerCommand = new RelayCommand(AddNewServer, true);

                return _addServerCommand;
            }
            set { }
        }
        public RelayCommand ToggleAddPanelCommand
        {
            get
            {
                if (_toggleAddPanelCommand == null)
                    _toggleAddPanelCommand = new RelayCommand(ToggleAddPanel, true);

                return _toggleAddPanelCommand;
            }
            set { }
        }
        public Server SelectedServer
        {
            get => _selectedServer;
            set => _selectedServer = value;
        }
        public Server NewServer
        {
            get
            {
                if (_newServer == null)
                    return _newServer = new Server();

                return _newServer;
            }
            set => _newServer = value;
        }
        public Visibility AddPanelVisibility
        {
            get => _addPanelVisibility;
            set
            {
                Set("AddPanelVisibility", ref _addPanelVisibility, value);
                _addPanelVisibility = value;
            }
        }

        private XMLHandler xmlHandler;
        private XElement serversRoot;

        public ServersVM(XMLHandler xmlHandler)
        {
            xmlHandler = xmlHandler;
            serversRoot = xmlHandler.LoadXmlFile("Servers.xml");

            Servers = SetupServersList();
        }

        private ObservableCollection<Server> SetupServersList()
        {
            try
            {
                var servers = serversRoot.Elements("Server").Select(
                    server => new Server
                    {
                        Name = server.Element("Name").Value,
                        Ip = server.Element("Ip").Value
                    }).ToList();

                return new ObservableCollection<Server>(servers);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }
        private void RemoveServer()
        {
            try
            {
                string name = SelectedServer.Name;
                string ip = SelectedServer.Ip;

                XElement serverToDelete = serversRoot.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);

                serverToDelete.Remove();
                serversRoot.Save("Servers.xml");

                Servers.Remove(Servers.Single(s => s.Name == name && s.Ip == ip));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void AddNewServer()
        {
            string name = _newServer.Name;
            string ip = _newServer.Ip;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ip))
            {
                MessengerInstance.Send(new ShowMessage(MessageType.AddServerError));
                return;
            }

            try
            {
                // check if server exists in db.
                serversRoot.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);
            }
            catch (InvalidOperationException ex)
            {
                XElement newServer = new XElement("Server");

                newServer.Add(new XElement("Name", name));
                newServer.Add(new XElement("Ip", ip));

                serversRoot.Add(newServer);
                serversRoot.Save("Servers.xml");

                Servers.Add(new Server { Name = name, Ip = ip });
            }
            finally
            {
                ToggleAddPanel();
            }
        }
        public void ToggleAddPanel() => AddPanelVisibility = AddPanelVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }
}
