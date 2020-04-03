using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace QueryToJson
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        private string[] functions = new string[] { "Information", "function2", "function3" };
        private Dictionary<string, string[]> jsonDefinitions;

        public MainWindow()
        {
            this.InitializeComponent();

            this.courseDataGrid.SelectionChanged += this.OnClickLogRow;

            //QueryDB(Constants.query);

            // Connection list view.
            foreach (string con in Constants.connections)
                this.connectionsLv.Items.Add(con);

            // Connection list view.
            foreach (string con in this.functions)
                this.functionsLv.Items.Add(con);
        }

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
            string query = this.queryTb.Text;

            this.QueryDB(query);
        }
        private void OnSelectFunction(object sender, SelectionChangedEventArgs e)
        {
            string func = e.AddedItems[0] as string;

            this.jsonFieldsLv.ItemsSource = Constants.d[func];
        }
        public void OnClickSend(object sender, RoutedEventArgs e)
        {
            string server = this.connectionsLv.SelectedItem as string;
            FormUrlEncodedContent jsonDefinitions = new FormUrlEncodedContent(new Dictionary<string, string>());

            var response = client.PostAsync("http://" + server + "/B1.SVC", jsonDefinitions);
        }
    }
}

public class Servers
{
    public ObservableCollection<string> Name { get; set; }
    public ObservableCollection<string> Ip { get; set; }

    public Servers(Collection<string> names, Collection<string> ips)
    {
        this.Name = names as ObservableCollection<string>;
        this.Ip = ips as ObservableCollection<string>;
    }

}

public static class Constants
{
    public const string query = "SELECT CreateDate, CreateTime, U_DeviceId, U_ActionType, U_Request, U_Response" +
                                " FROM [@ES_LOG]" +
                                " WHERE CreateDate = (select LEFT(convert(varchar, getdate(), 25), 11) + '00:00:00.000')" +
                                " ORDER BY CreateTime DESC";

    public const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

    public static readonly string[] connections = { "connection1", "connection2", "connection3" };

    public static readonly Dictionary<string, ObservableCollection<string>> d = new Dictionary<string, ObservableCollection<string>>
    {
        {
            "Information",
            new ObservableCollection<string>() { "DeviceId", "FCM_Token", "Location", "Token", "IsFullList", "Sync_From_Date", "Sync_From_Time" }
        }
    };
}



