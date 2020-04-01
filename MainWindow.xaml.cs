using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace QueryToJson
{
    public partial class MainWindow : Window
    {
        private const string connectionString = @"Data Source=SAP-DEV-2;Initial Catalog=SBODemoIL;Integrated Security=True";
        private string[] connections = new string[] { "connection1", "connection2", "connection3" };
        private string[] functions = new string[] { "Information", "function2", "function3" };
        private Dictionary<string, string[]> jsonDefinitions;
        private static readonly HttpClient client = new HttpClient();

        public MainWindow()
        {
            this.InitializeComponent();

            courseDataGrid.SelectionChanged += OnClickLogRow;

            string query = "SELECT CreateDate, CreateTime, U_DeviceId, U_ActionType, U_Request, U_Response" +
                           " FROM [@ES_LOG]" +
                           " WHERE CreateDate = (select LEFT(convert(varchar, getdate(), 25), 11) + '00:00:00.000')" +
                           " ORDER BY CreateTime DESC";

            QueryDB(query);

            // Connection list view.
            foreach (string con in connections)
                connectionsLv.Items.Add(con);

            // Connection list view.
            foreach (string con in functions)
                functionsLv.Items.Add(con);

            jsonDefinitions = new Dictionary<string, string[]> 
            { 
                { "Information", new string[] { "DeviceId", "FCM_Token", "Location", "Token", "IsFullList", "Sync_From_Date", "Sync_From_Time" }}
            };
        }

        private void QueryDB(string query)
        {
            using (SqlConnection sc = new SqlConnection(connectionString))
            {
                queryTb.Text = query;

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
                string request = row.Row["U_Request"] as string;
                JObject joRequest = JObject.Parse(request);
                this.requestDisplay.Text = joRequest.ToString();

                string response = row.Row["U_Response"] as string;
                JObject joResponse = JObject.Parse(response);
                this.responseDisplay.Text = joResponse.ToString();
            }
        }
        private void OnClickQuery(object sender, RoutedEventArgs e)
        {
            string query = queryTb.Text;

            QueryDB(query);
        }
        private void OnSelectFunction(object sender, SelectionChangedEventArgs e)
        {
            string func = e.AddedItems[0] as string;
            string[] jsonKeys = jsonDefinitions[func];

            foreach (string key in jsonKeys)
            {
                TextBlock tb = new TextBlock { Text = key + ":", Margin = new Thickness(5, 0, 5, 0) };
                TextBox tbx = new TextBox { Width = 100 };
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(tb);
                sp.Children.Add(tbx);
                jsonFieldsSp.Children.Add(sp);
            }
        }
    }
}


