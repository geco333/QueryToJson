using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryToJson.Model
{
    public class ApiFunction : ObservableObject
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
        public ObservableCollection<ApiFunctionParameter> Parameters { get; set; }
    }
}
