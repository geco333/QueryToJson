using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using QueryToJson.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace QueryToJson.ViewModel
{
    public class FunctionsVM : ViewModelBase, IAPIListView
    {
        private RelayCommand sendCommand;
        private ObservableCollection<ApiFunctionParameter> parameters = new ObservableCollection<ApiFunctionParameter>();
        private string selectedFunction;
        private string response;

        public RelayCommand SendCommand
        {
            get
            {
                if (sendCommand == null)
                    sendCommand = new RelayCommand(SendAPIRequest, true);

                return sendCommand;
            }
            set => sendCommand = value;
        }
        public XElement Root { get; set; }
        public ObservableCollection<string> Functions { get; set; }
        public ObservableCollection<ApiFunctionParameter> Parameters
        {
            get => parameters;
            set
            {
                Set("Parameters", ref parameters, value);
                parameters = value;
            }
        }
        public string SelectedFunction
        {
            get => selectedFunction;
            set
            {
                Set("SelectedFunction", ref selectedFunction, value);
                Parameters = GetSelectedFunctionsParameters(value);
                selectedFunction = value;
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

        public FunctionsVM(XElement root)
        {
            Root = root;
            Functions = GetFunctionsList();
        }

        private ObservableCollection<string> GetFunctionsList()
        {
            List<string> functionsNames = new List<string>();

            foreach (XElement function in Root.Elements("Function"))
            {
                string name = function.Element("Name").Value;
                functionsNames.Add(name);
            }

            return new ObservableCollection<string>(functionsNames);
        }
        private ObservableCollection<ApiFunctionParameter> GetSelectedFunctionsParameters(string functionName)
        {
            IEnumerable<XElement> function = Root.Elements("Function").Single(f => f.Element("Name").Value == functionName).Element("Parameters").Elements("Parameter");

            List<ApiFunctionParameter> parameters = function.Select(p => new ApiFunctionParameter { Key = p.Element("Key").Value, Value = p.Element("Value").Value }).ToList();

            return new ObservableCollection<ApiFunctionParameter>(parameters);
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

                    foreach (ApiFunctionParameter item in Parameters)
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
        private async void SendAPIRequest()
        {
            string body = BuildBody();

            using (HttpClient client = new HttpClient())
            using (StringContent sc = new StringContent(body, System.Text.Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = await client.PostAsync("http://urlecho.appspot.com/echo", sc);
                Response = response.ToString();
            }
        }
    }
}
