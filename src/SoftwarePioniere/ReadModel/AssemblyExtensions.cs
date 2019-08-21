using System;
using System.Linq;
using System.Reflection;

namespace SoftwarePioniere.ReadModel
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// returns all entitytypes from an assembly
        /// </summary>
        /// <returns></returns>
        public static string[] GetEntityTypesConstants(this Assembly assembly, Func<Type, bool> predicate = null)
        {
            if (predicate == null)
            {
                predicate = type => true;

            }

            return assembly.GetTypes().Where(x => !x.GetTypeInfo().IsAbstract && typeof(Entity).IsAssignableFrom(x) && predicate(x)).Select(x => ((Entity)Activator.CreateInstance(x)).EntityType.ToUpper()).ToArray();

        }

        /// <summary>
        /// Entity Type infos auslesen
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static EntityTypeInfo[] GetEntityTypeInfos(this Assembly assembly, Func<Type, bool> predicate = null)
        {
            if (predicate == null)
            {
                predicate = type => true;

            }
            return assembly.GetTypes().Where(x => !x.GetTypeInfo().IsAbstract && typeof(Entity).IsAssignableFrom(x) && predicate(x)).Select(x =>
                new EntityTypeInfo()
                {
                    Name = x.Name,
                    FullName = x.FullName,
                    TypeKey = ((Entity)Activator.CreateInstance(x)).EntityType.ToUpper()
                }
              ).ToArray();

        }


    }
}
