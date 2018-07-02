using System;
using System.Linq;
using System.Reflection;

namespace SoftwarePioniere.Messaging.Notifications
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// returns all entitytypes from an assembly
        /// </summary>
        /// <returns></returns>
        public static string[] GetNotificationContentTypesConstants(this Assembly assembly, Func<Type, bool> predicate = null)
        {
            if (predicate == null)
            {
                predicate = type => true;
            }

            return assembly.GetTypes().Where(x => !x.GetTypeInfo().IsAbstract && typeof(INotificationContent).IsAssignableFrom(x) && predicate(x)).Select(x => ((INotificationContent)Activator.CreateInstance(x)).NotificationType.ToUpper()).ToArray();

        }

        public static NotificationMessageTypeInfo[] GetNotificationContentTypesInfos(this Assembly assembly, Func<Type, bool> predicate = null)
        {
            if (predicate == null)
            {
                predicate = type => true;
            }

            return assembly.GetTypes().Where(x => !x.GetTypeInfo().IsAbstract && typeof(INotificationContent).IsAssignableFrom(x) && predicate(x)).Select(x => 
                new NotificationMessageTypeInfo()
                {
                    Name = x.Name,
                    FullName = x.FullName,
                    TypeKey = ((INotificationContent)Activator.CreateInstance(x)).NotificationType.ToUpper()
                }
            ).ToArray();

        }

    }
}
