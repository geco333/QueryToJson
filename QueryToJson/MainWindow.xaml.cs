using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace QueryToJson
{
    public partial class MainWindow : Window
    {
        private Grid mainGrid;
        private const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MySchool;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public MainWindow()
        {
            this.InitializeComponent();

            courseDataGrid.SelectionChanged += OnClickOnRow;

            using (SqlConnection sc = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM [Course]";

                SqlDataAdapter adapter = new SqlDataAdapter(query, sc);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                this.courseDataGrid.ItemsSource = ds.Tables[0].DefaultView;
            }

            HttpClient hc = new HttpClient();
            HttpContent hct = new HttpContent();
        }

        private void OnClickOnRow(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            DataRowView row = dg.SelectedItem as DataRowView;
            string json = row.Row[5] as string;

            JObject jo = JObject.Parse(json);

            this.jsonDisplay.Text = jo.ToString();
        }

        private Grid SetupMainGrid(int cols = 1, int rows = 1)
        {
            ColumnDefinition col1 = new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Auto) };
            ColumnDefinition col2 = new ColumnDefinition() { Width = new GridLength(7, GridUnitType.Star) };
            RowDefinition row1 = new RowDefinition();
            Grid grid = new Grid()
            {
                ColumnDefinitions = { col1, col2 },
                RowDefinitions = { row1 }
            };

            return grid;
        }
    }
}


