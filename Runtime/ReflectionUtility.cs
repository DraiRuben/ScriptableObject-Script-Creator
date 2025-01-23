using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ruben.SOCreator
{
    public static class ReflectionUtility
    {
        private static HashSet<string> _variableKeywords = new()
        {
            "void", "bool", "byte", "sbyte", "char", "decimal", "double", "float", "int", "uint", "long", "ulong", "object", "short", "ushort", "string"
        };

        public static Type GetClassWithName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).FirstOrDefault(type => type.Name == name);
        }

        public static bool IsVariableKeyword(string name)
        {
            return _variableKeywords.Contains(name);
        }

        public static List<Type> GetChildClasses(this Type parentType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(parentType)).OrderBy(x=>x.Name).ToList();
        }

        private const BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static List<FieldInfo> FetchAllFieldsWithAttribute<T>(this Type toFindOn) where T : Attribute
        {
            return toFindOn.GetFields(allFlags).Where(x => x.GetCustomAttribute<T>() != null).ToList();
        }
    }
}