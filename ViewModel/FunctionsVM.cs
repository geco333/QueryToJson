using QueryToJson.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace QueryToJson.ViewModel
{
    public class FunctionsVM
    {
        public IEnumerable<ApiFunctionParameter> Functions { get; set; }

        private XMLHandler xmlHandler = XMLHandler.Instance;
        private XElement functionsRoot;

        public FunctionsVM() => functionsRoot = xmlHandler.LoadXmlFile("ApiFunctions.xml");

        public void SelectFunction(string selectedFunction)
        {
            try
            {
                // Get the function name with respect to the function selected.
                XElement function = functionsRoot.Elements("Function").Single(f => f.Element("Name").Value == selectedFunction);

                // Get all the functions' parameters.
                IEnumerable<XElement> parameters = function.Elements("Parameters").Elements("Parameter");

                // Create a list of ApiFunctionParameter using the relevent functions' parameters.
                Functions = parameters.Select(
                p => new ApiFunctionParameter
                {
                    Key = p.Element("Key").Value,
                    Value = p.Element("Value").Value
                }
            );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
