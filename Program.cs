using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using Gibbed.IO;
using DriveConverter.Files;
using DriveConverter.XML;
using DriveConverter.Drive;
using DriveConverter.App;

namespace DriveConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Utility.Log.ToConsole(Config.VersionString);

            if (args.Length == 0)
            {
                Utility.Log.ToConsole(Config.UsageString);

                Environment.Exit(0);
            }

            if (Config.ProcessArgs(args) > 0)
            {
                Work();
            }
            else
            {
                throw new Exception("Invalid number of arguments");
            }

            Utility.Log.ToConsole("Done!");
            //Console.ReadKey(true);
        }

        static void Work()
        {
            string inputFileName = Config.Input;
            string inputFilePath = Path.GetFullPath(inputFileName);

            Convert(inputFilePath);

            //RetrieveParameters(inputFilePath);
        }

        static void Convert(string path)
        {
            string fileExtension = Path.GetExtension(path);

            switch (fileExtension.ToLower())
            {
                case ".bin":
                    if (Config.ExportProject == true)
                    {
                        ExportProject(path);
                    }
                    else
                    {
                        Export(path);
                    }
                    break;
                case ".xml":
                    Import(path);
                    break;
                case ".handling":
                    ImportProject(path);
                    break;
                default:
                    throw new NotSupportedException("Unknown input file extension");
            }
        }

        static void ImportProject(string path)
        {
            string inputString = File.ReadAllText(path);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(inputString);

            XmlSerializer serializer = new XmlSerializer(typeof(HandlingProject));

            HandlingProject handlingXml;

            using (XmlReader reader = new XmlNodeReader(xmlDocument))
            {
                handlingXml = (HandlingProject)serializer.Deserialize(reader);
            }

            Binary binary = new Binary();

            foreach (HandlingProjectParameter parameter in handlingXml.Parameter)
            {
                int key = -1;
                int type = -1;
                object value = null;

                if (!int.TryParse(parameter.Key, out key))
                {
                    throw new InvalidDataException("Invalid parameter key format");
                }

                foreach (KeyValuePair<uint, string> pair in Parameters.Types)
                {
                    if (pair.Value == parameter.Type)
                    {
                        type = (int)pair.Key;
                        break;
                    }
                }

                switch (type)
                {
                    case 1: // uint
                        value = uint.Parse(parameter.Value);
                        break;
                    case 2: // float
                        value = float.Parse(parameter.Value, CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new NotSupportedException("Unknown value type");
                }

                Console.WriteLine($"Key: {key} Type: {type} Value: {value}");

                if (key == -1 || type == -1 || value == null)
                {
                    throw new FormatException("Invalid parameter data");
                }

                binary.parameters.Add(new Binary.Parameter((uint)key, (uint)type, value));
            }

            string outputFilePath = Path.ChangeExtension(path, "handling.bin");

            FileStream outputStream = File.OpenWrite(outputFilePath);

            binary.Serialize(outputStream);
        }

        static void Import(string path)
        {
            string inputString = File.ReadAllText(path);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(inputString);

            XmlSerializer serializer = new XmlSerializer(typeof(HandlingConverted));

            HandlingConverted handlingXml;

            using (XmlReader reader = new XmlNodeReader(xmlDocument))
            {
                handlingXml = (HandlingConverted) serializer.Deserialize(reader);
            }

            if (string.IsNullOrEmpty(handlingXml.Type))
            {
                throw new InvalidDataException("Input file does not specify handling type");
            }

            Binary binary = new Binary();

            foreach (HandlingParameter parameter in handlingXml.Parameter)
            {
                int key = -1;
                int type = -1;
                object value = null;

                if (!int.TryParse(parameter.Key, out key))
                {
                    throw new InvalidDataException("Could not parse parameter key");

                    /*
                    foreach (KeyValuePair<uint, string> keys in Parameters.Keys[handlingXml.Type.ToLower()])
                    {
                        if (keys.Value == parameter.Key)
                        {
                            key = (int) keys.Key;

                            break;
                        }
                    }
                    */
                }

                foreach (KeyValuePair<uint, string> pair in Parameters.Types)
                {
                    if (pair.Value == parameter.Type)
                    {
                        type = (int) pair.Key;
                        break;
                    }
                }

                switch (type)
                {
                    case 1: // uint
                        value = uint.Parse(parameter.Value);
                        break;
                    case 2: // float
                        value = double.Parse(parameter.Value, CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new NotSupportedException("Unknown value type");
                }

                //Console.WriteLine($"Key: {key} Type: {type} Value: {value}");

                if (key == -1 || type == -1 || value == null)
                {
                    throw new FormatException("Invalid parameter data");
                }

                binary.parameters.Add(new Binary.Parameter((uint) key, (uint) type, value));
            }

            string outputFilePath = Path.ChangeExtension(path, null);

            FileStream outputStream = File.OpenWrite(outputFilePath);

            binary.Serialize(outputStream);
        }

        static void ExportProject(string path)
        {
            Stream inputStream = new MemoryStream(File.ReadAllBytes(path));

            Binary binary = new Binary();

            binary.Deserialize(inputStream);

            string outputFilePath = Path.ChangeExtension(Path.ChangeExtension(path, null), ".xml.handling");

            List<HandlingProjectParameter> handlingXmlParameters = new List<HandlingProjectParameter>();

            List<KeyValuePair<uint, string>> captures = new List<KeyValuePair<uint, string>>();

            foreach (KeyValuePair<string, Dictionary<uint, string>> types in Parameters.Keys)
            {
                uint numberOfMatches = 0;

                foreach (Binary.Parameter parameter in binary.parameters)
                {
                    if (types.Value.ContainsKey(parameter.Key))
                    {
                        numberOfMatches++;

                        string match = types.Value[parameter.Key];
                    }
                }

                captures.Add(new KeyValuePair<uint, string>(numberOfMatches, types.Key));
            }

            string mostCapturedType = captures.OrderByDescending(keySelector => keySelector.Key).First().Value;

            foreach (Binary.Parameter parameter in binary.parameters)
            {
                string key = parameter.Key.ToString();
                string type = parameter.Type.ToString();
                string value = System.Convert.ToString(parameter.Value, CultureInfo.InvariantCulture);

                HandlingProjectParameter item = new HandlingProjectParameter()
                {
                    Key = key,
                    Type = type,
                    Value = value
                };

                handlingXmlParameters.Add(item);
            }

            HandlingProject handlingXml = new HandlingProject()
            {
                ID = "860837444",
                Version = "2",
                Parameter = handlingXmlParameters.ToArray()
            };

            using (var writer = new StreamWriter(outputFilePath))
            {
                var serializer = new XmlSerializer(typeof(HandlingProject));

                using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true, NewLineOnAttributes = true }))
                {
                    serializer.Serialize(xmlWriter, handlingXml);

                    xmlWriter.Close();
                }

                writer.Close();
            }
        }

        static void Export(string path)
        {
            Stream inputStream = new MemoryStream(File.ReadAllBytes(path));

            Binary binary = new Binary();

            binary.Deserialize(inputStream);

            string outputFilePath = Path.ChangeExtension(path, "bin.xml");

            List<HandlingParameter> handlingXmlParameters = new List<HandlingParameter>();

            List<KeyValuePair<uint, string>> captures = new List<KeyValuePair<uint, string>>();

            foreach (KeyValuePair<string, Dictionary<uint, string>> types in Parameters.Keys)
            {
                uint numberOfMatches = 0;

                foreach (Binary.Parameter parameter in binary.parameters)
                {
                    if (types.Value.ContainsKey(parameter.Key))
                    {
                        numberOfMatches++;

                        string match = types.Value[parameter.Key];
                    }
                }

                captures.Add(new KeyValuePair<uint, string>(numberOfMatches, types.Key));
            }

            string mostCapturedType = captures.OrderByDescending(keySelector => keySelector.Key).First().Value;

            foreach (Binary.Parameter parameter in binary.parameters)
            {
                string key = parameter.Key.ToString();
                string name = Parameters.Keys[mostCapturedType].ContainsKey(parameter.Key) ? Parameters.Keys[mostCapturedType][parameter.Key] : null;
                string type = Parameters.Types[parameter.Type];
                string value = System.Convert.ToString(parameter.Value, CultureInfo.InvariantCulture); //parameter.Value.ToString();

                HandlingParameter item = new HandlingParameter()
                {
                    Key = key,
                    Name = name,
                    Type = type,
                    Value = value
                };

                handlingXmlParameters.Add(item);
            }

            HandlingConverted handlingXml = new HandlingConverted()
            {
                Type = mostCapturedType,
                Parameter = handlingXmlParameters.ToArray()
            };

            using (var writer = new StreamWriter(outputFilePath))
            {
                var serializer = new XmlSerializer(typeof(HandlingConverted));

                using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true, NewLineOnAttributes = true }))
                {
                    serializer.Serialize(xmlWriter, handlingXml);

                    xmlWriter.Close();
                }

                writer.Close();
            }
        }

        static void RetrieveParameters(string path)
        {
            string[] files = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);

            Console.WriteLine($"Searching {files.Length} files");

            Dictionary<string, Dictionary<uint, string>> types = new Dictionary<string, Dictionary<uint, string>>();

            foreach (string file in files)
            {
                XmlDocument doc = new XmlDocument();

                doc.Load(file);

                string handlingName = Path.GetFileNameWithoutExtension(file).Replace("ParameterDefinitions", ""); //doc.DocumentElement.SelectSingleNode("//Section").Attributes.GetNamedItem("Name").Value;

                Dictionary<uint, string> keys = new Dictionary<uint, string>();

                IterateChildren(doc.DocumentElement.ChildNodes, ref keys);

                types.Add(handlingName, keys);
            }

            using (var writer = File.CreateText(Path.GetFullPath("keys.txt")))
            {
                foreach (KeyValuePair<string, Dictionary<uint, string>> type in types)
                {
                    writer.WriteLine($"Vehicle Type: \"{type.Key}\"");

                    var keysOrdered = type.Value.OrderBy(key => key.Key);

                    foreach (KeyValuePair<uint, string> item in keysOrdered)
                    {
                        writer.WriteLine($"\tID: \"{item.Key}\" Value: \"{item.Value}\"");
                    }
                }

                writer.Close();
            }

            void IterateChildren(XmlNodeList nodes, ref Dictionary<uint, string> dictionary)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.Name == "Property")
                    {
                        string name = node.Attributes.GetNamedItem("DisplayName").Value;
                        uint index = uint.Parse(node.Attributes.GetNamedItem("Index").Value);

                        if (!dictionary.ContainsKey(index))
                        {
                            dictionary.Add(index, name);
                        }
                        else
                        {
                            string nameNew = name;
                            string nameOld = dictionary[index];

                            if (nameNew != nameOld)
                            {
                                Console.WriteLine($"Duplicate key: {index}\n\tExisting: \"{dictionary[index]}\"\n\tNew: \"{name}\"");
                            }
                        }

                        //Console.WriteLine($"{index}: \"{name}\"");
                    }

                    IterateChildren(node.ChildNodes, ref dictionary);
                }
            }
        }
    }
}
