using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Monkeyspeak.Utils
{
    public class ReflectionHelper
    {
        private static List<Assembly> all;

        public static Type[] GetAllTypesWithAttributeInMembers<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(type => type.GetMembers().Any(member => member.GetCustomAttribute<T>() != null)).ToArray();
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

        public static IEnumerable<Type> GetAllTypesWithBaseClass<T>(Assembly asm)
        {
            var desiredType = typeof(T);
            var types = new List<Type>();
            try
            {
                foreach (var type in GetAllTypesInAssembly(asm).Where(t => t.BaseType != null))
                {
                    if (GetAllBaseTypes(type).Contains(desiredType))
                        types.Add(type);
                }
            }
            catch (Exception ex)
            { }
            return types;
        }

        public static IEnumerable<Type> GetAllTypesWithInterface<T>(Assembly asm)
        {
            var desiredType = typeof(T);
            if (desiredType.IsInterface)
            {
                foreach (var type in GetAllTypesInAssembly(asm).Where(t => t.GetInterfaces().Contains(desiredType))) yield return type;
            }
        }

        public static IEnumerable<Type> GetAllTypesInAssembly(Assembly asm)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
            return types;
        }

        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            if (all != null && all.Count > 0) return all;
            all = new List<Assembly>();
            foreach (string asmFile in Directory.EnumerateFiles(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "*.*", SearchOption.TopDirectoryOnly)
                                        .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe")))
            {
                if (TryLoadAssemblyFromFile(asmFile, out var asm))
                {
                    if (all.AddIfUnique(asm))
                    {
                        Logger.Debug($"Caching: {asm.GetName().Name}");
                    }
                }
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // avoid all the Microsoft and System assemblies.  All assesmblies it is looking for should be in the local path
                if (asm.GlobalAssemblyCache) continue;

                if (all.AddIfUnique(asm)) Logger.Debug($"Caching: {asm.GetName().Name}");
            }
            return all;
        }

        public static bool HasNoArgConstructor(Type type)
        {
            return type.GetConstructors().FirstOrDefault(cnstr => cnstr.GetParameters().Length == 0) != null;
        }

        /// <summary>
        /// Tries the load assembly from file.
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <param name="asm">The asm.</param>
        /// <returns></returns>
        public static bool TryLoadAssemblyFromFile(string assemblyFilePath, out Assembly asm)
        {
            try
            {
                asm = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFilePath));
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
        /// <param name="asm">The asm.</param>
        /// <returns></returns>
        public static bool TryLoadAssemblyFromName(AssemblyName assemblyName, out Assembly asm)
        {
            try
            {
                asm = Assembly.Load(assemblyName);
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
    }
}