using GalaSoft.MvvmLight;
using System.Data;

namespace QueryToJson.Model
{
    public class Log : ObservableObject
    {
        private DataView db;
        private DataRowView selectedServer;
        private string request;
        private string response;

        public DataView DB
        {
            get => db;
            set
            {
                Set("DB", ref db, value);
                db = value;
            }
        }
        public DataRowView SelectedServer
        {
            get => selectedServer;
            set
            {
                Request = value["Json"] as string;
                selectedServer = value;
            }
        }
        public string Request
        {
            get => request;
            set
            {
                Set("Request", ref request, value);
                request = value;
            }
        }
        public string Response
        {
            get => response;
            set
            {
                Set("Response", ref response, value);
                response = value;
            }
        }
    }
}
