using System;
using System.Reflection;

namespace CuttingRoomFloor
{
    public static class Tools
    {
        public static void Log(object o, bool debug = false)
        {
            ETGModConsole.Log($"{o}", debug);
        }

        public static void LogError(object o, bool debug = false)
        {
            ETGModConsole.Log($"<color=red>{o}</color>", debug);
        }

        public static MethodInfo GetMethod(Type classType, string methodName, bool isStatic = false)
        {
            return classType.GetMethod(methodName, GetFlags(isStatic));
        }

        public static FieldInfo GetField(Type classType, string methodName, bool isStatic = false)
        {
            return classType.GetField(methodName, GetFlags(isStatic));
        }

        public static PropertyInfo GetProperty(Type classType, string propName, bool isStatic = false)
        {
            return classType.GetProperty(propName, GetFlags(isStatic));
        }

        public static BindingFlags GetFlags(bool isStatic)
        {
            return BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
        }

        public static T GetFieldValue<T>(Type classType, string fieldName, object o)
        {
            FieldInfo field = GetField(classType, fieldName, o == null);

            return (T)field.GetValue(o);
        }

        public static void SetFieldValue<T>(Type classType, string fieldName, T value, object o)
        {
            FieldInfo field = GetField(classType, fieldName, o == null);

            field.SetValue(o, value);
        }

        public static T GetPropertyValue<T>(Type classType, string propName, object o, object[] indexes = null)
        {
            PropertyInfo property = GetProperty(classType, propName, o == null);

            return (T)property.GetValue(o, indexes);
        }

        public static void SetProperty<T>(Type classType, string propName, T value, object o, object[] indexes = null)
        {
            PropertyInfo property = GetProperty(classType, propName, o == null);

            property.SetValue(o, value, indexes);
        }
    }
}