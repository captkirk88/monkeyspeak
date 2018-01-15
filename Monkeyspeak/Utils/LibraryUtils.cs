using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Extensions;
using Monkeyspeak.Libraries;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Utils
{
    /// <summary>
    /// Simple utility class for BaseLibrary instances
    /// </summary>
    public class LibraryUtils
    {
        /// <summary>
        /// Saves the handler mappings from the specified <paramref name="lib"/> to a file.
        /// </summary>
        /// <param name="lib">The library.</param>
        /// <param name="filePath">The file path.</param>
        public static void SaveHandlerMappings(BaseLibrary lib, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(lib.GetType().AssemblyQualifiedName);
            foreach (var handler in lib.Handlers)
            {
                sb.Append(handler.Key).Append('=').Append(handler.Value.Method.Name).Append('\n');
            }
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Saves the handler mappings from the specified <paramref name="old"/> library to map to the specified <paramref name="newLib"/> later.
        /// *Note* <paramref name="newLib"/> must be used when loading the mappings later on.
        /// </summary>
        /// <param name="lib">The library.</param>
        /// <param name="filePath">The file path.</param>
        public static void SaveHandlerMappings<T, U>(T old, U newLib, string filePath) where T : BaseLibrary where U : BaseLibrary
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(newLib.GetType().AssemblyQualifiedName);
            foreach (var handler in old.Handlers)
            {
                foreach (var newHandler in newLib.Handlers)
                {
                    if (newHandler.Key == handler.Key)
                    {
                        sb.AppendLine($"{handler.Key}={newHandler.Value.Method.Name}");
                    }
                }
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                ex.Log<LibraryUtils>();
            }
        }

        /// <summary>
        /// Loads the handler mappings from the file into the specified <paramref name="lib"/>.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="lib">The library.</param>
        /// <param name="filePath">The file path.</param>
        public static void LoadHandlerMappings<T>(MonkeyspeakEngine engine, T lib, string filePath) where T : BaseLibrary
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var type = lib.GetType();
                var mappingType = Type.GetType(lines[0]);
                if (mappingType != type) return;
                foreach (var line in lines.Skip(1))
                {
                    var trigger = Trigger.Parse(engine, line.LeftOf('='));
                    var handler = line.RightOf('=');
                    if (trigger == Trigger.Undefined) continue;
                    var method = type.GetMethod(handler, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreReturn, null, new Type[] { typeof(TriggerReader) }, null);
                    if (method == null)
                    {
                        Logger.Debug<LibraryUtils>($"method null with {handler} in {type.Name}");
                        continue;
                    }
                    lib.Add(trigger, method);
                    Logger.Debug<LibraryUtils>($"Registered {trigger} to {method.Name} in {type.Name}");
                }
            }
            catch (Exception ex)
            {
                ex.Log<LibraryUtils>();
            }
        }
    }
}