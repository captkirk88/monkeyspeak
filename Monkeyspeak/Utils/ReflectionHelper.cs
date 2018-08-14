using Monkeyspeak.Extensions;
using Monkeyspeak.Libraries;
using Monkeyspeak.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Monkeyspeak.Utils
{
    public sealed class ReflectionHelper
    {
        private static ConcurrentDictionary<Assembly, List<Type>> all = new ConcurrentDictionary<Assembly, List<Type>>();

        public static Type[] GetAllTypesWithAttribute<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(type => type.GetMembers().Any(member => member.GetCustomAttribute<T>() != null)).ToArray();
        }

        public static Type[] GetAllTypesWithAttribute<T>() where T : Attribute
        {
            return GetAllAssemblies().SelectMany(asm => asm.GetTypes()).Where(type => type.GetMembers().Any(member => member.GetCustomAttribute<T>() != null)).ToArray();
        }

        public static IEnumerable<T> GetAllAttributesFromMethod<T>(MethodInfo methodInfo) where T : Attribute
        {
            var attributes = methodInfo.GetCustomAttributes(true).OfType<T>().ToArray();
            if (attributes != null && attributes.Length > 0)
                for (int k = 0; k <= attributes.Length - 1; k++)
                {
                    yield return attributes[k];
                }
        }

        public static IEnumerable<MethodInfo> GetAllMethods(Type type, params Type[] args)
        {
            MethodInfo[] methods = type.GetMethods();
            for (int j = 0; j <= methods.Length - 1; j++)
            {
                var @params = methods[j].GetParameters();
                if (args.Length != @params.Length) continue;
                bool paramsMatch = true;
                for (int i = @params.Length - 1; i >= 0; i--)
                {
                    if (@params[i].ParameterType != args[i])
                    {
                        paramsMatch = false;
                        break;
                    }
                }
                if (paramsMatch)
                    yield return methods[j];
            }
        }

        public static IEnumerable<Type> GetAllBaseTypes(Type type)
        {
            if (type.IsAbstract || type.IsInterface) yield break;
            var baseType = type;
            while (baseType != typeof(Object))
            {
                baseType = baseType.BaseType;
                yield return baseType;
            }
        }

        internal static object GetPropertyValue(object target, string desiredProperty)
        {
            var propInfo = target.GetType().GetProperty(desiredProperty);
            if (propInfo != null && propInfo.GetMethod != null && propInfo.CanRead)
                return propInfo.GetValue(target);
            return null;
        }

        public static void SetPropertyValue(object target, string desiredProperty, object value)
        {
            var propInfo = target.GetType().GetProperty(desiredProperty);
            if (propInfo != null && propInfo.SetMethod != null && propInfo.CanWrite)
                propInfo.SetValue(target, value);
        }

        public static IEnumerable<Type> GetAllTypesWithBaseClass<T>(Assembly asm, string ns = null)
        {
            var desiredType = typeof(T);
            var types = new List<Type>();
            foreach (var type in GetAllTypesInAssembly(asm).Where(t => GetAllBaseTypes(t).Contains(desiredType) && (ns != null ? t.Namespace.StartsWith(ns) : true)))
            {
                yield return type;
            }
        }

        public static IEnumerable<Type> GetAllTypesWithBaseClass<T>(string ns = null)
        {
            var desiredType = typeof(T);
            var types = new List<Type>();
            foreach (var type in GetAllAssemblies().SelectMany(asm => GetAllTypesInAssembly(asm))
                .Where(t => GetAllBaseTypes(t).Contains(desiredType) && (ns != null ? t.Namespace.StartsWith(ns) : true)))
            {
                yield return type;
            }
        }

        public static bool HasInterface<T>(Type type)
        {
            var desiredType = typeof(T);
            if (desiredType.IsInterface)
            {
                return type.GetInterface(desiredType.Name, true) != null;
            }
            return false;
        }

        public static IEnumerable<Type> GetAllTypesWithInterface<T>(Assembly asm, string ns = null)
        {
            var desiredType = typeof(T);
            if (desiredType.IsInterface)
            {
                foreach (var type in GetAllTypesInAssembly(asm)
                    .Where(t => t.GetInterfaces().Contains(desiredType) && (ns != null ? t.Namespace.StartsWith(ns) : true))) yield return type;
            }
        }

        public static IEnumerable<Type> GetAllTypesWithInterface<T>(string ns = null)
        {
            var desiredType = typeof(T);
            if (desiredType.IsInterface)
            {
                foreach (var type in GetAllAssemblies().SelectMany(asm => GetAllTypesInAssembly(asm))
                    .Where(t => t.GetInterfaces().Contains(desiredType) && (ns != null ? t.Namespace.StartsWith(ns) : true))) yield return type;
            }
        }

        /// <summary>
        /// Gets all types in assembly.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllTypesInAssembly(Assembly asm)
        {
            if (all.ContainsKey(asm) && all[asm].Count > 0) return all[asm];
            var types = new List<Type>(1000);
            try
            {
                types.AddRange(asm.GetTypes());
            }
            catch (ReflectionTypeLoadException e)
            {
                types.AddRange(e.Types.Where(type => type != null));
            }
            all[asm] = types;
            return types;
        }

        /// <summary>
        /// Gets all assemblies.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            if (all != null && all.Count > 0) return all.Keys;
            var asms = new List<Assembly>();
            foreach (string asmFile in Directory.EnumerateFiles(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "*.*", SearchOption.TopDirectoryOnly)
                                        .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe")))
            {
                if (TryLoadAssemblyFromFile(asmFile, out var asm))
                {
                    asms.AddIfUnique(asm);
                }
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // avoid all the Microsoft and System assemblies. All assesmblies it is looking for
                // should be in the local path
                if (asm.GlobalAssemblyCache) continue;

                asms.AddIfUnique(asm);
            }
            foreach (var asm in asms)
            {
                all.TryAdd(asm, new List<Type>());
            }
            return asms;
        }

        /// <summary>
        /// Determines whether [the specified type] has a no-arg constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// <c>true</c> if [the specified type] has a no-arg constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasNoArgConstructor(Type type)
        {
            return type.GetConstructors().FirstOrDefault(cnstr => cnstr.GetParameters().Length == 0) != null;
        }

        /// <summary>
        /// Tries the load assembly from file.
        /// </summary>
        /// <param name="assemblyFile">    The assembly file.</param>
        /// <param name="asm">             The asm.</param>
        /// <param name="assemblyFilePath">todo: describe assemblyFilePath parameter on TryLoadAssemblyFromFile</param>
        /// <returns></returns>
        public static bool TryLoadAssemblyFromFile(string assemblyFilePath, out Assembly asm)
        {
            try
            {
                var resolveMe = new ResolveEventHandler((o, args) =>
                {
                    Console.WriteLine($"{args.RequestingAssembly.FullName} wants {args.Name}");
                    return Assembly.ReflectionOnlyLoad(args.Name);
                });
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveMe;
                asm = Assembly.ReflectionOnlyLoad(AssemblyName.GetAssemblyName(assemblyFilePath).FullName);
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveMe;
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch
#endif
            {
                asm = null;
                return false;
            }
        }

        /// <summary>
        /// Tries the load assembly.
        /// </summary>
        /// <param name="assemblyName">The assembly string.</param>
        /// <param name="asm">         The asm.</param>
        /// <returns></returns>
        public static bool TryLoadAssemblyFromName(AssemblyName assemblyName, out Assembly asm)
        {
            try
            {
                var resolveMe = new ResolveEventHandler((o, args) =>
                {
                    Console.WriteLine($"{args.RequestingAssembly.FullName} wants {args.Name}");
                    return Assembly.ReflectionOnlyLoad(args.Name);
                });
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveMe;
                asm = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveMe;
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch
#endif
            {
                asm = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to create the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="obj"> The object.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static bool TryCreate<T>(Type type, out T obj, params object[] args)
        {
            if (!type.IsAbstract && !type.IsInterface)
            {
                try
                {
                    if (args == null || args.Length == 0)
                        obj = (T)Activator.CreateInstance(type);
                    else obj = (T)Activator.CreateInstance(type, args);
                    return obj != null;
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }
            obj = default(T);
            return false;
        }

        /// <summary>
        /// Tries to create the specfied type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"> The object.</param>
        /// <param name="args">The arguments.</param>
        /// <returns><c>true</c> if type was created; otherwise <c>false</c></returns>
        public static bool TryCreate<T>(out T obj, params object[] args)
        {
            if (TryCreate(typeof(T), out object result, args))
            {
                obj = (T)result;
                return true;
            }
            obj = default(T);
            return false;
        }

        /// <summary>
        /// Creates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static T Create<T>(params object[] args)
        {
            var type = typeof(T);
            if (!type.IsAbstract && !type.IsInterface)
            {
                if (args == null || args.Length == 0)
                    return (T)Activator.CreateInstance(type);
                else return (T)Activator.CreateInstance(type, args);
            }
            return default(T);
        }

        /// <summary>
        /// Creates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static object Create(Type type, params object[] args)
        {
            if (!type.IsAbstract && !type.IsInterface)
            {
                if (args == null || args.Length == 0)
                    return Activator.CreateInstance(type);
                else return Activator.CreateInstance(type, args);
            }
            return null;
        }

        public static string GetMethodDefinition<T>(string methodName)
        {
            var mi = typeof(T).GetRuntimeMethods().FirstOrDefault(m => m.Name.Contains(methodName));
            return MethodBase.GetMethodFromHandle(mi.MethodHandle).ToString();
        }

        public static string GetMethodDefinition(MethodInfo mi)
        {
            return MethodBase.GetMethodFromHandle(mi.MethodHandle).ToString();
        }
    }
}