using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PixelArt {
    public static class XnaSerializer
    {
        public static void Serialize<T>(string absolutePath, T data)
        {

            var serializer = new XmlSerializer(data.GetType());
            using (var writer = XmlWriter.Create(absolutePath))
            {
                serializer.Serialize(writer, data);
            }
        }

        public static T Deserialize<T>(string absolutePath)
        {
            T data;

            var serializer = new XmlSerializer(typeof(T));

            using (var reader = XmlReader.Create(absolutePath))
            {
                var test = (T)serializer.Deserialize(reader);
                data = test;
            }

            return data;
        }
    }

    /*public static class DataSerializer {
        public static void Serialize<T>(string fileName, T data) {
            ObjectSerializer.Serialize<T>(Paths.dataPath + fileName + ".xml", data);
        }
        
        public static T Deserialize<T>(string fileName) {
            return ObjectSerializer.Deserialize<T>(Paths.dataPath + fileName + ".xml");
        }
    }*/
}