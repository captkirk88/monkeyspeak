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
            var attributes = methodInfo.GetCustomAttributes<T>(true).ToArray();
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
            var types = new Type[0];
            try
            {
                types = asm.GetTypes();
            }
            catch (Exception ex)
            { yield break; }
            foreach (var type in types)
            {
                if (GetAllBaseTypes(type).Contains(desiredType))
                    yield return type;
            }
        }

        public static IEnumerable<Type> GetAllTypesWithInterface<T>(Assembly asm)
        {
            var desiredType = typeof(T);
            if (!desiredType.IsInterface) yield break;
            var types = new Type[0];
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Logger.Error($"Failed to load types from {asm.GetName().Name}");
                foreach (var loaderEx in ex.LoaderExceptions) loaderEx.Log();
                yield break;
            }
            foreach (var type in types.Where(i => i.GetInterfaces().Contains(desiredType)))
                yield return type;
        }

        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            if (all != null && all.Count > 0) return all;
            all = new List<Assembly>();
            foreach (string asmFile in Directory.EnumerateFiles(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "*.*", SearchOption.TopDirectoryOnly)
                                        .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe")))
            {
                var asm = Assembly.LoadFile(asmFile);
                all.AddIfUnique(asm);
                foreach (var referenced in asm.GetReferencedAssemblies())
                    all.AddIfUnique(Assembly.Load(referenced));
            }

            if (Assembly.GetExecutingAssembly() != null)
            {
                // this detects the path from where the current CODE is being executed
                foreach (var asmName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    all.AddIfUnique(Assembly.Load(asmName));
                }
            }
            else if (Assembly.GetEntryAssembly() != null)
            {
                // this detects the path from where the current CODE is being executed
                foreach (var asmName in Assembly.GetEntryAssembly().GetReferencedAssemblies())
                {
                    all.AddIfUnique(Assembly.Load(asmName));
                }
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // avoid all the Microsoft and System assemblies.  All assesmblies it is looking for should be in the local path
                if (asm.GlobalAssemblyCache) continue;

                all.AddIfUnique(asm);
            }
            //foreach (var asm in all) Logger.Debug<ReflectionHelper>(asm.GetName().Name);
            return all;
        }

        public static bool HasNoArgConstructor(Type type)
        {
            return type.GetConstructors().FirstOrDefault(cnstr => cnstr.GetParameters().Length == 0) != null;
        }

        public static bool TryLoad(string assemblyFile, out Assembly asm)
        {
            try
            {
                asm = Assembly.LoadFile(Path.GetFullPath(assemblyFile));
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
                try
                {
                    if (args == null || args.Length == 0)
                        return Activator.CreateInstance(type);
                    else return Activator.CreateInstance(type, args);
                }
                catch (Exception ex)
                {
                }
            }
            return null;
        }
    }
}