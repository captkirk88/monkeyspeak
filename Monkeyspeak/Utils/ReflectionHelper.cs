using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Monkeyspeak.Utils
{
    internal class ReflectionHelper
    {
        public static Type[] GetAllTypesWithAttributeInMembers<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(type => type.GetMembers().Any(member => member.GetCustomAttribute<T>() != null)).ToArray();
        }

        public static IEnumerable<T> GetAllAttributesFromMethod<T>(MethodInfo methodInfo) where T : Attribute
        {
            var attributes = (T[])methodInfo.GetCustomAttributes(typeof(T), false);
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

        public static IEnumerable<Type> GetAllTypesWithBaseClass<T>(Assembly asm) where T : class
        {
            var desiredType = typeof(T);
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(desiredType))
                    yield return type;
            }
        }

        public static bool HasNoArgConstructor(Type type)
        {
            return type.GetConstructors().Any(cnstr => cnstr.GetParameters().Length == 0);
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
#if DEBUG
                throw ex;
#else
                return false;
#endif
            }
        }
    }
}