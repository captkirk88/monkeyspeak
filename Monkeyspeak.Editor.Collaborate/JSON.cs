#region Usings

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Monkeyspeak.Utils;
using System.Linq;
using Monkeyspeak.Logging;
using Monkeyspeak.Extensions;

#endregion Usings

namespace Monkeyspeak.Editor.Collaborate
{
    public class JSON
    {
        private static JsonSerializer serializer;

        public static void WarmUp()
        {
            new JSON();
        }

        static JSON()
        {
            serializer = new JsonSerializer
            {
                Culture = CultureInfo.InvariantCulture,
                ConstructorHandling = ConstructorHandling.Default,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.Objects,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                StringEscapeHandling = StringEscapeHandling.Default,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTime,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                FloatParseHandling = FloatParseHandling.Double,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            serializer.Converters.Add(new VersionConverter());
            serializer.Converters.Add(new XmlNodeConverter());
            serializer.Converters.Add(new StringEnumConverter());
            serializer.Converters.Add(new KeyValuePairConverter());
            serializer.Converters.Add(new JavaScriptDateTimeConverter());

            foreach (var type in ReflectionHelper.GetAllAssemblies().SelectMany(asm => ReflectionHelper.GetAllTypesWithBaseClass<JsonConverter>(asm).Where(t => t.Namespace.StartsWith("Monkeyspeak"))))
            {
                var inst = ReflectionHelper.Create(typeof(JsonConverter)) as JsonConverter;
                if (inst == null) continue;
                serializer.Converters.Add(inst);
                Logger.Debug<JSON>($"Registered custom converter {inst.GetType().FullName}");
            }

            serializer.Error += (sender, args) => Logger.Error<JSON>($"Member: {args.ErrorContext.Member} Path: {args.ErrorContext.Path}\n{args.ErrorContext.Error}");
        }

        public static void AddConverter<T>(T converter) where T : JsonConverter
        {
            if (converter != null)
                serializer.Converters.Add(converter);
        }

        public static void Serialize(object obj, Stream output, TypeNameHandling typeHandling, bool readable = true)
        {
            if (output == null)
                return;
            try
            {
                using (var writer = new JsonTextWriter(new StreamWriter(output, Encoding.UTF8))
                {
                    QuoteName = false,
                    Formatting = readable ? Formatting.Indented : Formatting.None
                })
                {
                    var oldTypeHandling = serializer.TypeNameHandling;
                    serializer.TypeNameHandling = typeHandling;
                    serializer.Serialize(writer, obj);
                    serializer.TypeNameHandling = oldTypeHandling;
                    writer.Flush();
                    output.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
        }

        public static void Serialize(object obj, Stream output, bool readable = true)
        {
            Serialize(obj, output, serializer.TypeNameHandling, readable);
        }

        public static string Serialize(object obj, TypeNameHandling typeHandling = TypeNameHandling.Objects, JsonConverter converter = null, bool readable = true)
        {
            try
            {
                StringBuilder output = new StringBuilder();
                using (var writer = new StringWriter(output))
                using (var jwriter = new JsonTextWriter(writer)
                {
                    QuoteName = false,
                    Formatting = readable ? Formatting.Indented : Formatting.None
                })
                {
                    var oldTypeHandling = serializer.TypeNameHandling;
                    serializer.TypeNameHandling = typeHandling;
                    serializer.Serialize(writer, obj);
                    serializer.TypeNameHandling = oldTypeHandling;
                    writer.Flush();
                }
                return output.ToString();
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
            return null;
        }

        public static object Deserialize(Stream input)
        {
            if (input == null || !input.CanRead)
                return null;
            try
            {
                using (input)
                using (var reader = new JsonTextReader(new StreamReader(input)))
                {
                    var result = serializer.Deserialize(reader);
                    return result;
                }
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
            finally
            {
                input.Close();
            }
            return null;
        }

        public static T Deserialize<T>(Stream input)
        {
            if (input == null || !input.CanRead)
                return default(T);
            try
            {
                using (input)
                using (var reader = new JsonTextReader(new StreamReader(input)))
                {
                    var result = serializer.Deserialize<T>(reader);
                    return result;
                }
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
            finally
            {
                input.Close();
            }
            return default(T);
        }

        public static object Deserialize(string jsonStr)
        {
            if (jsonStr.IsNullOrBlank())
                return null;
            try
            {
                using (var reader = new JsonTextReader(new StringReader(jsonStr)))
                {
                    var result = serializer.Deserialize(reader);
                    return result;
                }
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
            return null;
        }

        public static T Deserialize<T>(string jsonStr)
        {
            if (jsonStr.IsNullOrBlank())
                return default(T);
            try
            {
                using (var reader = new JsonTextReader(new StringReader(jsonStr)))
                {
                    var result = serializer.Deserialize<T>(reader);
                    return result;
                }
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
            return default(T);
        }

        public static object Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;
            try
            {
                using (var mem = new MemoryStream(data))
                using (var reader = new JsonTextReader(new StreamReader(mem)))
                {
                    var result = serializer.Deserialize(reader);
                    return result;
                }
            }
            catch (NotImplementedException nil) { }
            catch (Exception ex)
            {
                ex.Log<JSON>();
            }
            return null;
        }
    }
}