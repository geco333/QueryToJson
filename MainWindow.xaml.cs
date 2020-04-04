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
        private readonly HttpClient client = new HttpClient();
        private XMLHandler xmlHandler = XMLHandler.Instance;

        public MainWindow()
        {
            this.InitializeComponent();

            this.xmlHandler.Window = this;

            this.QueryDB(Constants.query);

            this.addServerPanel.Visibility = Visibility.Collapsed;
            this.serverMessageBlock.Visibility = Visibility.Collapsed;
            this.pasteFromClipboard.Visibility = Visibility.Collapsed;

            this.LoadServersList();
            this.xmlHandler.FillApiFunctionsListView();
        }

        private async void PostToApi(string body)
        {
            string serverIp = (this.connectionsLv.SelectedValue as Server).Ip;

            StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await this.client.PostAsync("http://" + serverIp + "/B1.SVC", content);
        }
        private void ToggleAddServerPanel()
        {
            this.addServerNameBox.Text = string.Empty;
            this.addServerIpBox.Text = string.Empty;

            this.addServerPanel.Visibility = Visibility.Collapsed;

            this.addServerBtn.Visibility = Visibility.Visible;
            this.delServerBtn.Visibility = Visibility.Visible;
        }
        private void LoadServersList() => this.xmlHandler.FillServersListView();

        private void QueryDB(string query)
        {
            using (SqlConnection sc = new SqlConnection(Constants.connectionString))
            {
                this.queryTb.Text = query;

                SqlDataAdapter adapter = new SqlDataAdapter(query, sc);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                this.courseDataGrid.ItemsSource = ds.Tables[0].DefaultView;
            }
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

                //string response = row.Row["U_Response"] as string;
                //JObject joResponse = JObject.Parse(response);
                //this.responseDisplay.Text = joResponse.ToString();
            }
        }
        private void OnClickQuery(object sender, RoutedEventArgs e)
        {
            string query = this.queryTb.Text;

            this.QueryDB(query);
        }
        private void OnSelectFunction(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get the ApiFunctions.xml file.
                XElement apiFunctionXml = XElement.Load("ApiFunctions.xml");

                // Get the function name with respect to the function selected.
                XElement function = apiFunctionXml.Elements("Function").Single(f => f.Element("Name").Value == (string)e.AddedItems[0]);

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

                this.jsonFieldsLv.ItemsSource = apiFunctionParametersList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        private void OnClickSend(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            try
            {
                using (JsonWriter jr = new JsonTextWriter(sw))
                {
                    jr.Formatting = Formatting.Indented;

                    jr.WriteStartObject();

                    foreach (ApiFunctionParameter item in this.jsonFieldsLv.Items)
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

            this.PostToApi(sb.ToString());
        }
        private void OnClickCopyToApi(object sender, RoutedEventArgs e)
        {
            string json = this.requestDisplay.Text;

            if (!string.IsNullOrEmpty(json))
                Constants.ClipBoard = JObject.Parse(json);
        }
        private void OnClickPasteFromClipboard(object sender, RoutedEventArgs e)
        {
            foreach (ApiFunctionParameter item in jsonFieldsLv.Items)
            {
                if (Constants.ClipBoard.ContainsKey(item.Key))
                {
                    item.Value = (string)Constants.ClipBoard[item.Key];
                    jsonFieldsLv.Items.Refresh();
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
                this.xmlHandler.AddNewServer(name, ip);
                this.LoadServersList();

                this.ToggleAddServerPanel();
            }
        }
        private void OnClickDelServerBtn(object sender, RoutedEventArgs e)
        {
            string name = (this.connectionsLv.SelectedItem as Server).Name;
            string ip = (this.connectionsLv.SelectedItem as Server).Ip;

            this.xmlHandler.RemoveServer(name, ip);

            this.LoadServersList();
        }

        public void ShowMessage(MessageType messageType)
        {
            TextBlock messageBlock = this.serverMessageBlock;

            switch (messageType)
            {
                case MessageType.AddServer:
                    messageBlock = this.serverMessageBlock;
                    messageBlock.Text = "Server added.";
                    break;

                case MessageType.AddServerError:
                    messageBlock = this.serverMessageBlock;
                    messageBlock.Text = "Error: server name and ip already exists.";
                    break;

                case MessageType.DelServer:
                    messageBlock = this.serverMessageBlock;
                    messageBlock.Text = "Server deleted.";
                    break;

                default:
                    break;
            }

            messageBlock.Visibility = Visibility.Visible;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (object sender, EventArgs e) =>
            {
                messageBlock.Visibility = Visibility.Collapsed;
                (sender as DispatcherTimer).Stop();
            };

            timer.Start();
        }
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
    public static class Constants
    {
        public const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MySchool;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public const string query = "SELECT * FROM [dbo].[Course]";

        public static JObject ClipBoard;
    }

    public class XMLHandler
    {
        public static XMLHandler Instance => new XMLHandler();
        public MainWindow Window;

        private XMLHandler()
        {

        }

        public void FillServersListView()
        {
            try
            {
                XElement serversXml = XElement.Load("Servers.xml");
                IEnumerable<Server> servers = serversXml.Elements("Server").Select(
                    server => new Server
                    {
                        Name = server.Element("Name").Value,
                        Ip = server.Element("Ip").Value
                    });

                this.Window.connectionsLv.ItemsSource = servers;
                this.Window.connectionsLv.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        public void FillApiFunctionsListView()
        {
            try
            {
                XElement apiFunctionXml = XElement.Load("ApiFunctions.xml");
                IEnumerable<string> apiFunctions = apiFunctionXml.Elements("Function").Select(function => function.Element("Name").Value);

                this.Window.functionsLv.ItemsSource = apiFunctions;
                this.Window.functionsLv.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        public void AddNewServer(string name, string ip)
        {
            XElement serversXml = XElement.Load("Servers.xml");

            try
            {
                serversXml.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);
                this.Window.ShowMessage(MessageType.AddServerError);
                return;
            }
            catch (System.InvalidOperationException ex)
            {
            }

            XElement newServer = new XElement("Server");

            newServer.Add(new XElement("Name", name));
            newServer.Add(new XElement("Ip", ip));

            serversXml.Add(newServer);
            serversXml.Save("Servers.xml");

            this.Window.ShowMessage(MessageType.AddServer);
        }
        public void RemoveServer(string name, string ip)
        {
            XElement serversXml = XElement.Load("Servers.xml");

            // TODO: Fix duplicate names.

            XElement serverToDelete = serversXml.Elements("Server").Single(x => x.Element("Name").Value == name && x.Element("Ip").Value == ip);

            serverToDelete.Remove();

            serversXml.Save("Servers.xml");
        }
    }
}