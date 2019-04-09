using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SoftwarePioniere.Util
{
    /// <summary>
    ///     Hilfsklasse für die Erstellung einer Report Table aus Objekten
    /// </summary>
    public class ReportTableHelper
    {
        Dictionary<string, DataColumn> _columns;
        Dictionary<Type, IEnumerable<PropertyInfo>> _props;

        /// <summary>
        ///     Die erzeuge Datatable
        /// </summary>
        public DataTable Table { get; private set; }

        /// <summary>
        ///     Erstellt aus dem Objekttype die Spalten für die Tabelle
        /// </summary>
        /// <param name="objectType"></param>
        public void CreateTableHeader(Type objectType)
        {
            CreateTableHeader(objectType, string.Empty);
        }

        /// <summary>
        ///     Erstellt aus dem angegebenen Objekttype die Spalten für eine Datattable
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="prefix">Prefix für die Tabellenspalten</param>
        public void CreateTableHeader(Type objectType, string prefix)
        {
            if (Table == null)
                Table = new DataTable();

            if (_columns == null)
                _columns = new Dictionary<string, DataColumn>();

            if (_props == null)
                _props = new Dictionary<Type, IEnumerable<PropertyInfo>>();


            foreach (var p in GetProps(objectType))
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
                var col = Table.Columns.Add(cName, ctype ?? throw new InvalidOperationException());
                var key = objectType.FullName + "_" + p.Name;
                _columns.Add(key, col);
            }
        }

        /// <summary>
        ///     Die erzeugten Rows füllen
        /// </summary>
        /// <param name="objectWithValues"></param>
        public DataRow FillTableRow(params object[] objectWithValues)
        {
            var row = Table.NewRow();

            foreach (var obj in objectWithValues)
            {

                if (obj != null)
                {
                    var t = obj.GetType();

                    var props = GetProps(t);
                    {
                        foreach (var p in props)
                        {
                            var key = t.FullName + "_" + p.Name;
                            var col = _columns[key];

                            //wenn enum, dann tostring, sonst
                            var v = p.GetValue(obj, null);
                            if (v == null)
                                row[col] = DBNull.Value;
                            else
                                row[col] = v;
                        }
                    }
                }
            }

            Table.Rows.Add(row);

            return row;
        }


        IEnumerable<PropertyInfo> GetProps(Type t)
        {
            if (_props.ContainsKey(t))
                return _props[t];


            var list = new List<PropertyInfo>();

            //alle properties auslesen
            foreach (var p in t.GetProperties())
            {
                if (p.PropertyType.Namespace != null
                    &&
                    (((p.PropertyType.IsPublic) && (p.PropertyType.Namespace.ToLower() == "system")) ||
                     (p.PropertyType.IsEnum)))
                {
                    list.Add(p);
                }
                //if (p.PropertyType.BaseType.FullName.ToLower() == "system.enum" && p.PropertyType.IsPublic)
                //{
                //  list.RegisterEffect(p);
                //}
            }

            _props.Add(t, list);

            return list;
        }


        // ReSharper disable once UnusedMember.Local
        void CreateRowForObject(DataTable table, object objectWithValue, string prefix)
        {
            if (objectWithValue == null)
                return;

            var objectType = objectWithValue.GetType();

            var row = table.NewRow();

            foreach (var p in GetProps(objectType))
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
    }
}