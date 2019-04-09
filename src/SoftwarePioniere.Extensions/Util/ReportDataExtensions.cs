using System;
using System.Data;
using System.Linq;

namespace SoftwarePioniere.Util
{
    static class ReportDataExtensions
    {
        /// <summary>
        /// Fügt die Spalten für einen Typ in die DataTable ein
        /// </summary>
        /// <param name="table"></param>
        /// <param name="objectType"></param>
        public static void CreateColumnsForType(this DataTable table, Type objectType)
        {
            CreateColumnsForType(table, objectType, string.Empty);
        }

        /// <summary>
        /// Fügt die Spalten für einen Typ in die DataTable ein
        /// Dabei wird vor jede Spalte ein Prefix gesetzt
        /// </summary>
        /// <param name="table"></param>
        /// <param name="objectType"></param>
        /// <param name="prefix"></param>
        public static void CreateColumnsForType(this DataTable table, Type objectType, string prefix)
        {

            var props = objectType.GetProperties().Where(
                p => p.PropertyType.Namespace != null
                    && (
                    (p.PropertyType.IsPublic && p.PropertyType.Namespace.ToLower() == "system")
                    ||
                     (p.PropertyType.IsEnum))
                );


            foreach (var p in props)
            {
                string cName;
                if (!string.IsNullOrEmpty(prefix))
                    cName = prefix + "_" + p.Name;
                else
                    cName = p.Name;

                var ctype = p.PropertyType;

                var isNullable = ctype.IsGenericType && ctype.GetGenericTypeDefinition() == typeof(Nullable<>);

                if (isNullable)
                {
                    ctype = Nullable.GetUnderlyingType(ctype);
                }
                else
                {
                    //wenn enum, dann typ string
                    if (p.PropertyType.IsEnum)
                        ctype = typeof(string);
                }

                table.Columns.Add(cName, ctype ?? throw new InvalidOperationException());
            }
        }


        /// <summary>
        /// Fügt das Objekt in die DataTable ein, Es wird einen neue Row erzeugt
        /// Für jede Spalte wird das Prefix verwendet
        /// </summary>
        /// <param name="table"></param>
        /// <param name="objectWithValue"></param>
        /// <param name="prefix"></param>
        public static void CreateRowForObject(this DataTable table, object objectWithValue, string prefix)
        {
            if (objectWithValue == null)
                return;

            var objectType = objectWithValue.GetType();
            var props = objectType.GetProperties().Where(
              p => p.PropertyType.Namespace != null
                  && (
                  (p.PropertyType.IsPublic && p.PropertyType.Namespace.ToLower() == "system")
                  ||
                   (p.PropertyType.IsEnum))
              );

            var row = table.NewRow();

            foreach (var p in props)
            {
                string cName;
                if (!string.IsNullOrEmpty(prefix))
                    cName = prefix + "_" + p.Name;
                else
                    cName = p.Name;


                //wenn enum, dann tostring, sonst
                var v = p.GetValue(objectWithValue, null);
                if (v == null)
                    row[cName] = DBNull.Value;
                else
                    row[cName] = v;

            }

            table.Rows.Add(row);
        }


        /// <summary>
        /// Fügt das Objekt in die DataTable ein, Es wird einen neue Row erzeugt
        /// </summary>
        /// <param name="table"></param>
        /// <param name="objectWithValue"></param>
        public static void CreateRowForObject(this DataTable table, object objectWithValue)
        {
            CreateRowForObject(table, objectWithValue, string.Empty);
        }
    }
}
