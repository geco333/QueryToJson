using System;
using System.Xml.Linq;

namespace QueryToJson.Model
{
    interface IAPIListView
    {
        XElement Root { get; set; }
    }
}
