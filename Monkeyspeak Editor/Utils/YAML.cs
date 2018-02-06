using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpYaml.Serialization;

namespace Monkeyspeak.Editor.Utils
{
    public static class YAML
    {
        private static Serializer serializer;

        private static Dictionary<Type, IYamlSerializable> customSerializers = new Dictionary<Type, IYamlSerializable>();
        private static SerializerSettings settings;

        static YAML()
        {
            var schema = new SharpYaml.Schemas.ExtendedSchema();
            settings = new SerializerSettings(schema)
            {
                DefaultStyle = SharpYaml.YamlStyle.Flow,
                EmitAlias = false,
                EmitTags = true,
                EmitDefaultValues = true,
                EmitJsonComptible = false,
                EmitShortTypeName = false,
                SerializeDictionaryItemsAsMembers = true,
            };
            serializer = new Serializer(settings);
        }

        public static void AddSerializer<T>(IYamlSerializable serializable)
        {
            settings.RegisterSerializer(typeof(T), serializable);
        }

        public static void AddSerializer(Type type, IYamlSerializable serializable)
        {
            settings.RegisterSerializer(type, serializable);
        }

        public static void SerializeToFile(object obj, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, obj, obj.GetType());
            }
        }

        public static string SerializeToString(object obj)
        {
            return serializer.Serialize(obj, obj.GetType());
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default(T);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return (T)serializer.Deserialize(stream, typeof(T));
                }
            }
            catch (Exception ex) { ex.Log(); return default(T); }
        }

        public static T DeserializeFromFile<T>(string filePath, T existingObject)
        {
            if (!File.Exists(filePath)) return existingObject;
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    return serializer.DeserializeInto(reader.ReadToEnd(), existingObject);
                }
            }
            catch (Exception ex) { ex.Log(); return existingObject; }
        }

        public static T DeserializeFromString<T>(string yaml)
        {
            if (string.IsNullOrWhiteSpace(yaml)) return default(T);
            try
            {
                return (T)serializer.Deserialize(yaml, typeof(T));
            }
            catch (Exception ex) { ex.Log(); return default(T); }
        }

        public static T DeserializeFromString<T>(string yaml, T existingObject)
        {
            if (string.IsNullOrWhiteSpace(yaml)) return existingObject;
            try
            {
                return serializer.DeserializeInto(yaml, existingObject);
            }
            catch (Exception ex) { ex.Log(); return existingObject; }
        }
    }
}