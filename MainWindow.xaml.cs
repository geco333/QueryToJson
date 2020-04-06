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
            this.serversList = new ServersList(this, "Servers.xml");
            this.functionsList = new FunctionsList(this, "ApiFunctions.xml");
            this.apiHandler = new APIHandler(this);

            this.dbHandler.SendQuery();

            this.addServerPanel.Visibility = Visibility.Collapsed;
            this.messageBar.Visibility = Visibility.Collapsed;
            this.pasteFromClipboard.Visibility = Visibility.Collapsed;

            this.functionsList.FillListView();
            this.serversList.FillListView();
        }

        private void ToggleAddServerPanel()
        {
            this.addServerNameBox.Text = string.Empty;
            this.addServerIpBox.Text = string.Empty;

            this.addServerPanel.Visibility = Visibility.Collapsed;

            this.addServerBtn.Visibility = Visibility.Visible;
            this.delServerBtn.Visibility = Visibility.Visible;
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
        }
        private void OnClickSend(object sender, RoutedEventArgs e)
        {
            string body = this.functionsList.BuildBody();
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
            }
        }
        private void OnClickDelServerBtn(object sender, RoutedEventArgs e)
        {
            string name = (this.connectionsLv.SelectedItem as Server).Name;
            string ip = (this.connectionsLv.SelectedItem as Server).Ip;

            this.serversList.RemoveServer(name, ip);
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
                this.mw.queryTb.Text = query;

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

    public abstract class FromXML
    {
        private string xmlFileName;
        protected XElement root;

        public FromXML(string xmlFileName)
        {
            this.xmlFileName = xmlFileName;
            this.LoadXmlFile();
        }

        private void LoadXmlFile() => this.root = XElement.Load(this.xmlFileName);
        public abstract void FillListView();
    }
    public class ServersList : FromXML
    {
        private MainWindow mw;
        private List<Server> servers;

        public ServersList(MainWindow mw, string xmlFileName) : base(xmlFileName) => this.mw = mw;

        public override void FillListView()
        {
            try
            {
                this.servers = base.root.Elements("Server").Select(
                    server => new Server
                    {
                        Name = server.Element("Name").Value,
                        Ip = server.Element("Ip").Value
                    }).ToList();

                this.mw.connectionsLv.ItemsSource = this.servers;
                this.mw.connectionsLv.SelectedIndex = 0;
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
                base.root.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);
                this.mw.ShowMessage(MessageType.AddServerError);
                return;
            }
            catch (System.InvalidOperationException ex)
            {
            }

            XElement newServer = new XElement("Server");

            newServer.Add(new XElement("Name", name));
            newServer.Add(new XElement("Ip", ip));

            base.root.Add(newServer);
            base.root.Save("Servers.xml");

            this.servers.Add(new Server { Name = name, Ip = ip });

            this.mw.connectionsLv.Items.Refresh();

            this.mw.ShowMessage(MessageType.AddServer);
        }
        public void RemoveServer(string name, string ip)
        {
            XElement serverToDelete = base.root.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);

            serverToDelete.Remove();
            base.root.Save("Servers.xml");

            this.servers.Remove(this.servers.Single(s => s.Name == name && s.Ip == ip));

            this.mw.connectionsLv.Items.Refresh();
        }
    }
    public class FunctionsList : FromXML
    {
        private MainWindow mw;

        public FunctionsList(MainWindow mw, string xmlFileName) : base(xmlFileName) => this.mw = mw;

        public override void FillListView()
        {
            try
            {
                IEnumerable<string> apiFunctions = base.root.Elements("Function").Select(function => function.Element("Name").Value);

                this.mw.functionsLv.ItemsSource = apiFunctions;
                this.mw.functionsLv.SelectedIndex = 0;
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
                XElement function = base.root.Elements("Function").Single(f => f.Element("Name").Value == (string)selectedFunction);

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

                this.mw.jsonFieldsLv.ItemsSource = apiFunctionParametersList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        public string BuildBody()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            try
            {
                using (JsonWriter jr = new JsonTextWriter(sw))
                {
                    jr.Formatting = Formatting.Indented;

                    jr.WriteStartObject();

                    foreach (ApiFunctionParameter item in this.mw.jsonFieldsLv.Items)
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
    }
}