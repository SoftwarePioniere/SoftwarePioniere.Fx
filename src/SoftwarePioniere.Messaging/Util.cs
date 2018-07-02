using System;
using System.Reflection;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Diverse Utils
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// Den Namen eines Events aus dem Typen ermitteln
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string EventTypeToEventName(Type type)
        {
            // return type.FullName.Replace(".", "_");
            return type.Name;
        }

        /// <summary>
        /// Den Namen eines Events aus dem Typen ermitteln
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetEventName(this Type type)
        {
            return EventTypeToEventName(type);
        }

        public static string GetTypeShortName(this Type type)
        {
            return $"{type.FullName}, {type.GetTypeInfo().Assembly.GetName().Name}";
        }

        /// <summary>
        /// Erzeugt einen Type anhand des ShortTypes
        /// </summary>
        /// <param name="typeShortName"></param>
        /// <returns></returns>
        public static Type CreateType(string typeShortName)
        {
            return Type.GetType(typeShortName, false);
        }


        /// <summary>
        /// Erzeugt einen Type anhand des ShortTypes 
        /// Prüft, ob der Name ohne Assembly anhand der Assembly aufgelöst werden kann
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeShortName"></param>
        /// <returns></returns>
        public static Type CreateType(Assembly assembly, string typeShortName)
        {
            var typ = CreateType(typeShortName);

            if (typ != null)
                return typ;

            var typeFullName = typeShortName.Split(',')[0];
            return assembly.GetType(typeFullName, true);
        }

    }
}
