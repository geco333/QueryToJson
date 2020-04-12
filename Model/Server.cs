using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace QueryToJson.Model
{
    public class Server : ObservableObject
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                Set("Name", ref name, value);
                name = value;
            }
        }
        private string ip;
        public string Ip
        {
            get => ip;
            set
            {
                Set("Ip", ref ip, value);
                ip = value;
            }
        }
    }
}
