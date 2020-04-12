using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueryToJson.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace QueryToJson
{
    public enum MessageType { AddServer, AddServerError, DelServer }

    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void ToggleAddServerPanel()
        {
            addServerNameBox.Text = string.Empty;
            addServerIpBox.Text = string.Empty;

            addServerPanel.Visibility = addServerPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            addServerBtn.Visibility = addServerBtn.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            delServerBtn.Visibility = delServerBtn.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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

        private void OnClickSend(object sender, RoutedEventArgs e)
        {
            string body = BuildBody();
        }
        private void OnClickCopyToApi(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(json))
            //    Constants.ClipBoard = JObject.Parse(json);
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
        private void OnTabFocusChange(object sender, SelectionChangedEventArgs e) => pasteFromClipboard.Visibility = Constants.ClipBoard is null ? Visibility.Collapsed : Visibility.Visible;
        private void OnClickAddServerButton(object sender, RoutedEventArgs e) => ToggleAddServerPanel();
        private void OnClickCencelAddServer(object sender, RoutedEventArgs e) => ToggleAddServerPanel();
        private void OnClickConfirmAddServer(object sender, RoutedEventArgs e)
        {
            string name = addServerNameBox.Text;
            string ip = addServerIpBox.Text;

            if (name != string.Empty && ip != string.Empty)
            {
                ToggleAddServerPanel();
            }

        }
        private void OnClickDelServerBtn(object sender, RoutedEventArgs e)
        {

        }
    }

    public static class Constants
    {
        public const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MySchool;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public const string SelectAllDefaultQuery = "SELECT * FROM [dbo].[Course]";

        public static JObject ClipBoard;
    }

    public class APIHandler
    {
        private MainWindow mw;
        private readonly HttpClient client = new HttpClient();

        public APIHandler(MainWindow mw) => this.mw = mw;


        public async void Send(string body)
        {
            StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
    public class XMLHandler
    {
        private static XMLHandler instance = null;
        public static XMLHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new XMLHandler();

                return instance;
            }
        }

        private XMLHandler() { }

        public XElement LoadXmlFile(string fileName) => XElement.Load(fileName);
    }
}