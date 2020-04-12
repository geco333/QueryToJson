using GalaSoft.MvvmLight;
using QueryToJson.Model;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Controls;

namespace QueryToJson.ViewModel
{
    public class LogVM : ViewModelBase
    {
        private Log log = new Log();
        public Log Log
        {
            get => log;
            set
            {
                log = value;
                Set("Log", ref log, value);
            }
        }

        public LogVM() => SendQuery();

        public void SendQuery(string query = Constants.SelectAllDefaultQuery)
        {
            using (SqlConnection sc = new SqlConnection(Constants.connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, sc);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                Log.DB = ds.Tables[0].DefaultView;
            }
        }
    }
}
