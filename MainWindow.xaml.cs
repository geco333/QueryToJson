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

        private void OnTabFocusChange(object sender, SelectionChangedEventArgs e) => pasteFromClipboard.Visibility = Constants.ClipBoard is null ? Visibility.Collapsed : Visibility.Visible;
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
}