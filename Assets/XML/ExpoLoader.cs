using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CourseXpo
{
    public partial class Expo
    {
        public static Expo Load(string filepath)
        {
            return XpoLoader.LoadXmlObject<Expo>(filepath);
        }
    }

    public static class XpoLoader
    {
        /// <summary>
        /// Loads an XML file into an XML serialized object
        /// </summary>
        /// <typeparam name="T">Type of XML object</typeparam>
        /// <param name="filepath">Path to the XML file</param>
        /// <returns>a new XML object of the type specified in memory</returns>
        public static T LoadXmlObject<T>(string filepath)
        {
            var fileStream = new FileStream(filepath, FileMode.Open);
            var value = Deserialize<T>(fileStream);
            fileStream.Close();
            return value;
        }

        /// <summary>
        /// Deserializes an XML object from a file
        /// </summary>
        /// <typeparam name="T">Type of XML object</typeparam>
        /// <param name="stream">Stream to extract the XML object from</param>
        /// <returns>Deserialized XML object from file</returns>
        private static T Deserialize<T>(Stream stream)
        {
            var reader = new StreamReader(stream);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);
        }
    }
}
