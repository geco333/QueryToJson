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
    public class ServersVM : ViewModelBase, IAPIListView
    {
        private RelayCommand _deleteServerCommand;
        private RelayCommand _addServerCommand;
        private RelayCommand _toggleAddPanelCommand;
        private Server _selectedServer;
        private Server _newServer;
        private Visibility _addPanelVisibility = Visibility.Collapsed;

        public XElement Root { get; set; }
        public ObservableCollection<Server> Servers { get; set; }
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
            set
            {
                Set("SelectedServer", ref _selectedServer, value);
                _selectedServer = value;
            }
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

        public ServersVM(XElement root)
        {
            Root = root;

            // Setup the servers list listview.
            Servers = SetupServersList();
        }

        private ObservableCollection<Server> SetupServersList()
        {
            try
            {
                var servers = Root.Elements("Server").Select(
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

                XElement serverToDelete = Root.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);

                serverToDelete.Remove();
                Root.Save("Servers.xml");

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
                MessengerInstance.Send(new ShowMessageBar(MessageType.AddServerError));
                return;
            }

            try
            {
                // check if server exists in db.
                Root.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);
            }
            catch (InvalidOperationException ex)
            {
                XElement newServer = new XElement("Server");

                newServer.Add(new XElement("Name", name));
                newServer.Add(new XElement("Ip", ip));

                Root.Add(newServer);
                Root.Save("Servers.xml");

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
