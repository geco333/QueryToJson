using System.Xml.Linq;

namespace QueryToJson.Model
{
    public class XMLHandler
    {
        private static XMLHandler instance = null;
        public static XMLHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new XMLHandler();

                return instance;
            }
        }

        private XMLHandler() { }

        public XElement LoadXmlFile(string fileName) => XElement.Load(fileName);
    }
}
