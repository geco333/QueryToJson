using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace QueryToJson.ViewModel
{
    public class ParametersVM
    {
        private IEnumerable<string> parameters;
        private XElement parametersRoot;
        private XMLHandler xmlHandler = XMLHandler.Instance;

        public IEnumerable<string> Parameters
        {
            get => parameters;
            set => parameters = value;
        }

        public ParametersVM() => parametersRoot = xmlHandler.LoadXmlFile("ApiFunctions.xml");

        public void FillFunctionsListView()
        {
            try
            {
                parameters = parametersRoot.Elements("Function").Select(function => function.Element("Name").Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
