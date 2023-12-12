using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Group1_Part3_HW5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Shablon method pattern in work:");

            DataProcessor xmlProcessor = new XmlDataProcessor();
            DataProcessor jsonProcessor = new JsonDataProcessor();
            DataProcessor delimitedProcessor = new DelimitedDataProcessor();

            string xmlData = @"<person>
                                   <name>Tom</name>
                                   <age>25</age>
                                   <hobby>Reading</hobby>
                               </person>";
            string jsonData = @"{
                                    ""name"": ""Alice"",
                                    ""age"": 23,
                                    ""skills"": [
                                    ""Programming"",
                                    ""Designing""
                                    ]
                                }";
            string delimitedData = @"John,Smith,30
Mary,Jones,28
Bob,Williams,35";

            xmlProcessor.ProcessData(xmlData);
            //jsonProcessor.ProcessData(jsonData);
            //delimitedProcessor.ProcessData(delimitedData);
        }
    }
    // Abstract class 
    abstract class DataProcessor
    {
        // Shablon method
        public void ProcessData(string data)
        {
            // Check if data is empty
            if (string.IsNullOrEmpty(data))
            {
                Console.WriteLine("No data to process");
                return;
            }

            // Get data in proper format
            var parsedData = ParseData(data);

            // Processing data
            var processedData = ProcessParsedData(parsedData);

            // Save data
            var list = SaveDataToList(processedData);
            //var list = SaveDataToList(data);

            // Print data
            Console.WriteLine("Processed data:");
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public abstract object ParseData(string data);

        public abstract object ProcessParsedData(object data);

        public List<object> SaveDataToList(object data)
        {
            Type type1 = data.GetType();
            var list = new List<object>();
            if (data != null)
                ////if (data is JsonDocument json)
                //{
                //    string jsonStr = JsonConvert.SerializeObject(data);
                //    //list = JsonConvert.DeserializeObject<List<Person>>(jsonStr);
                //}
                //else
                {

                    list.Add(data);
                }
            return list;
        }
    }

    // Concrete class to process XML format
    class XmlDataProcessor : DataProcessor
    {
        public override object ParseData(string data)
        {
            var xml = new XmlDocument();
            xml.LoadXml(data);
            return xml;
        }

        public override object ProcessParsedData(object data)
        {
            if (data is XmlDocument xml)
            {
                var json = new System.IO.StringWriter();
                var writer = new JsonTextWriter(json);
                AddElements(xml.DocumentElement, writer);
                //return new JsonData(json.ToString());
                return json; 
                    //System.Text.Json.JsonSerializer.Serialize(json);

            }
            else
            {
                return null;
            }
        }
        private void AddElements(XmlElement xmlElement, JsonWriter writer)
        {
            if (xmlElement.HasChildNodes)
            {
                writer.WritePropertyName(xmlElement.Name);
                writer.WriteStartObject();
                foreach (XmlNode node in xmlElement.ChildNodes)
                {
                    switch (node.NodeType)
                    {
                        case XmlNodeType.Element:
                            AddElements(node as XmlElement, writer);
                            break;
                        case XmlNodeType.Text:
                            writer.WritePropertyName(node.ParentNode.Name.ToString());
                            writer.WriteValue(node.Value);
                            break;
                    }
                }
                writer.WriteEndObject();
            }
            else
            {
                writer.WritePropertyName(xmlElement.Name);
                writer.WriteNull();
            }
        }

    }

    // Concrete class to process JSON format 
    class JsonDataProcessor : DataProcessor
    {
        public override object ParseData(string data)
        {
            var json = JsonDocument.Parse(data);
            return json;
        }

        public override object ProcessParsedData(object data)
        {
            if (data is JsonDocument json)
            {
                var xml = new XmlDocument();
                var root = xml.CreateElement("root");
                xml.AppendChild(root);
                AddElements(json.RootElement, root, xml);
                var xmlString = xml.OuterXml;

                return xmlString;
            }
            else
            {
                return null;
            }
        }

        private void AddElements(JsonElement jsonElement, XmlElement xmlElement, XmlDocument xml)
        {
            foreach (var property in jsonElement.EnumerateObject())
            {
                var newElement = xml.CreateElement(property.Name);
                xmlElement.AppendChild(newElement);
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        newElement.InnerText = property.Value.ToString();
                        break;
                    case JsonValueKind.Array:
                        foreach (var item in property.Value.EnumerateArray())
                        {
                            var itemElement = xml.CreateElement("item");
                            newElement.AppendChild(itemElement);
                            AddElements(item, itemElement, xml);
                        }
                        break;
                    case JsonValueKind.Object:
                        AddElements(property.Value, newElement, xml);
                        break;
                    case JsonValueKind.Null:
                        break;
                }
            }
        }
    }

    // Concrete class to process text format with delimiters
    class DelimitedDataProcessor : DataProcessor
    {
        public override object ParseData(string data)
        {
            var reader = new System.IO.StringReader(data);
            var list = new List<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                list.Add(line);
            }
            return list;
        }

        public override object ProcessParsedData(object data)
        {
            if (data is List<string> list)
            {
                var newList = new List<string>();
                foreach (var line in list)
                {
                    var parts = line.Split(',');
                    var newLine = string.Join(' ', parts.Reverse());
                    newList.Add(newLine);
                }
                return newList;
            }
            else
            {
                return null;
            }
        }
    }
    // Class to provide data in XML format
    class XmlData : Data
    {
        public override string Content { get; set; }

        public XmlData(string content)
        {
            Content = content;
        }

        public override void Display()
        {
            Console.WriteLine("XML data:");
            Console.WriteLine(Content);
        }
    }
    // Class to provide data in JSON format
    class JsonData : Data
    {
        public override string Content { get; set; }

        public JsonData(string content)
        {
            Content = content;
        }

        public override void Display()
        {
            Console.WriteLine("JSON data:");
            Console.WriteLine(Content);
        }
    }
    abstract class Data
    {
        public abstract string Content { get; set; }
        public abstract void Display();
    }
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string hobby { get; set; }
    }
}   