using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;

namespace QueryToJson
{
    public enum MessageType { AddServer, AddServerError, DelServer }

    public partial class MainWindow : Window
    {
        private DBHandler dbHandler;
        private ServersList serversList;
        private FunctionsList functionsList;
        private APIHandler apiHandler;

        public MainWindow()
        {
            this.InitializeComponent();

            this.dbHandler = new DBHandler(this);
            this.serversList = new ServersList();
            this.functionsList = new FunctionsList();
            this.apiHandler = new APIHandler(this);

            this.dbHandler.SendQuery();

            this.addServerPanel.Visibility = Visibility.Collapsed;
            this.messageBar.Visibility = Visibility.Collapsed;
            this.pasteFromClipboard.Visibility = Visibility.Collapsed;

            queryTb.Text = Constants.SelectAllDefaultQuery;

            this.functionsList.FillListView();
            this.serversList.FillListView();

            functionsLv.ItemsSource = functionsList.Parameters;
            functionsLv.SelectedIndex = 0;

            connectionsLv.ItemsSource = serversList.Servers;
            connectionsLv.SelectedIndex = 0;
        }

        private void ToggleAddServerPanel()
        {
            this.addServerNameBox.Text = string.Empty;
            this.addServerIpBox.Text = string.Empty;

            this.addServerPanel.Visibility = Visibility.Collapsed;

            this.addServerBtn.Visibility = Visibility.Visible;
            this.delServerBtn.Visibility = Visibility.Visible;
        }
        private string BuildBody()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            try
            {
                using (JsonWriter jr = new JsonTextWriter(sw))
                {
                    jr.Formatting = Formatting.Indented;

                    jr.WriteStartObject();

                    foreach (ApiFunctionParameter item in jsonFieldsLv.Items)
                    {
                        jr.WritePropertyName(item.Key);
                        jr.WriteValue(item.Value);
                    }

                    jr.WriteEndObject();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            return sb.ToString();
        }

        private void OnClickLogRow(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            DataRowView row = dg.SelectedItem as DataRowView;

            if (row != null)
            {
                string request = row.Row["Json"] as string;
                JObject joRequest = JObject.Parse(request);
                this.requestDisplay.Text = joRequest.ToString();
            }
        }
        private void OnClickQuery(object sender, RoutedEventArgs e)
        {
            string query = this.queryTb.Text;
            this.dbHandler.SendQuery(query);
        }
        private void OnSelectFunction(object sender, SelectionChangedEventArgs e)
        {
            string selectedFunction = e.AddedItems[0] as string;
            this.functionsList.SelectFunction(selectedFunction);

            jsonFieldsLv.ItemsSource = functionsList.ApiFunctionParametersList;
        }
        private void OnClickSend(object sender, RoutedEventArgs e)
        {
            string body = BuildBody();
            this.apiHandler.Send(body);
        }
        private void OnClickCopyToApi(object sender, RoutedEventArgs e)
        {
            string json = this.requestDisplay.Text;

            if (!string.IsNullOrEmpty(json))
                Constants.ClipBoard = JObject.Parse(json);
        }
        private void OnClickPasteFromClipboard(object sender, RoutedEventArgs e)
        {
            foreach (ApiFunctionParameter item in this.jsonFieldsLv.Items)
            {
                if (Constants.ClipBoard.ContainsKey(item.Key))
                {
                    item.Value = (string)Constants.ClipBoard[item.Key];
                    this.jsonFieldsLv.Items.Refresh();
                }
            }
        }
        private void OnTabFocusChange(object sender, SelectionChangedEventArgs e) => this.pasteFromClipboard.Visibility = Constants.ClipBoard is null ? Visibility.Collapsed : Visibility.Visible;
        private void OnClickAddServerButton(object sender, RoutedEventArgs e)
        {
            this.addServerPanel.Visibility = Visibility.Visible;
            this.addServerBtn.Visibility = Visibility.Hidden;
            this.delServerBtn.Visibility = Visibility.Hidden;
        }
        private void OnClickCencelAddServer(object sender, RoutedEventArgs e) => this.ToggleAddServerPanel();
        private void OnClickConfirmAddServer(object sender, RoutedEventArgs e)
        {
            string name = this.addServerNameBox.Text;
            string ip = this.addServerIpBox.Text;

            if (name != string.Empty && ip != string.Empty)
            {
                this.serversList.AddNewServer(name, ip);
                this.ToggleAddServerPanel();
                ShowMessage(MessageType.AddServerError);
            }

            connectionsLv.Items.Refresh();
            ShowMessage(MessageType.AddServer);
        }
        private void OnClickDelServerBtn(object sender, RoutedEventArgs e)
        {
            string name = (this.connectionsLv.SelectedItem as Server).Name;
            string ip = (this.connectionsLv.SelectedItem as Server).Ip;

            this.serversList.RemoveServer(name, ip);

            connectionsLv.Items.Refresh();
        }

        public void ShowMessage(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.AddServer:
                    this.messageBar.Text = "Server added.";
                    break;

                case MessageType.AddServerError:
                    this.messageBar.Text = "Error: server name and ip already exists.";
                    break;

                case MessageType.DelServer:
                    this.messageBar.Text = "Server deleted.";
                    break;

                default:
                    break;
            }

            this.messageBar.Visibility = Visibility.Visible;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (object sender, EventArgs e) =>
            {
                this.messageBar.Visibility = Visibility.Collapsed;
                (sender as DispatcherTimer).Stop();
            };

            timer.Start();
        }
    }

    public static class Constants
    {
        public const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MySchool;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public const string SelectAllDefaultQuery = "SELECT * FROM [dbo].[Course]";

        public static JObject ClipBoard;
    }

    public class Server
    {
        public string Name { get; set; }
        public string Ip { get; set; }
    }
    public class ApiFunctionParameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class DBHandler
    {
        private MainWindow mw;

        public DBHandler(MainWindow mw) => this.mw = mw;

        public void SendQuery(string query = Constants.SelectAllDefaultQuery)
        {
            using (SqlConnection sc = new SqlConnection(Constants.connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, sc);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                this.mw.courseDataGrid.ItemsSource = ds.Tables[0].DefaultView;
            }
        }
    }
    public class APIHandler
    {
        private MainWindow mw;
        private readonly HttpClient client = new HttpClient();

        public APIHandler(MainWindow mw) => this.mw = mw;


        public async void Send(string body)
        {
            string serverIp = (this.mw.connectionsLv.SelectedValue as Server).Ip;

            StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await this.client.PostAsync("http://" + serverIp + "/B1.SVC", content);
        }
    }
    public static class XMLHandler
    {
        public static XElement LoadXmlFile(string fileName) => XElement.Load(fileName);
    }
    public class ServersList
    {
        private XElement root = XMLHandler.LoadXmlFile("Servers.xml");
        private List<Server> servers;
        public List<Server> Servers => servers;

        public void FillListView()
        {
            try
            {
                this.servers = root.Elements("Server").Select(
                    server => new Server
                    {
                        Name = server.Element("Name").Value,
                        Ip = server.Element("Ip").Value
                    }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void AddNewServer(string name, string ip)
        {
            try
            {
                root.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);
                return;
            }
            catch (System.InvalidOperationException ex)
            {
            }

            XElement newServer = new XElement("Server");

            newServer.Add(new XElement("Name", name));
            newServer.Add(new XElement("Ip", ip));

            root.Add(newServer);
            root.Save("Servers.xml");

            this.servers.Add(new Server { Name = name, Ip = ip });
        }
        public void RemoveServer(string name, string ip)
        {
            XElement serverToDelete = root.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);

            serverToDelete.Remove();
            root.Save("Servers.xml");

            this.servers.Remove(this.servers.Single(s => s.Name == name && s.Ip == ip));
        }
    }
    public class FunctionsList
    {
        private XElement root = XMLHandler.LoadXmlFile("ApiFunctions.xml");
        private IEnumerable<string> parameters;
        public IEnumerable<string> Parameters => parameters;
        private IEnumerable<ApiFunctionParameter> apiFunctionParametersList;
        public IEnumerable<ApiFunctionParameter> ApiFunctionParametersList => apiFunctionParametersList;

        public void FillListView()
        {
            try
            {
                parameters = root.Elements("Function").Select(function => function.Element("Name").Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        public void SelectFunction(string selectedFunction)
        {
            try
            {
                // Get the function name with respect to the function selected.
                XElement function = root.Elements("Function").Single(f => f.Element("Name").Value == selectedFunction);

                // Get all the functions' parameters.
                IEnumerable<XElement> parameters = function.Elements("Parameters").Elements("Parameter");

                // Create a list of ApiFunctionParameter using the relevent functions' parameters.
                IEnumerable<ApiFunctionParameter> apiFunctionParametersList = parameters.Select(
                    p => new ApiFunctionParameter
                    {
                        Key = p.Element("Key").Value,
                        Value = p.Element("Value").Value
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}