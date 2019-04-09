using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoftwarePioniere.Util
{
    /// <summary>
    ///     Daten für einen Report erzeugen
    ///     Erstellt aus einem Objektbaum ein DataSet
    /// </summary>
    public class ReportDataHelper
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReportDataHelper" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ReportDataHelper(string name)
        {
            DataSet = new DataSet(name);
        }

        /// <summary>
        ///     Das erzeugte DataSet
        /// </summary>
        public DataSet DataSet { get; }

        /// <summary>
        ///     Ein Element eines Typs einfügen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Add<T>(T item)
        {
            AddItems(new List<T> {item});
        }

        /// <summary>
        ///     Ein Element eines Typs einfügen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="tableName"></param>
        public void Add<T>(T item, string tableName)
        {
            AddItems(new List<T> {item}, tableName);
        }

        /// <summary>
        ///     Eine Liste von Elementen in eine neue Table einfügen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectMany"></param>
        public void AddItems<T>(IEnumerable<T> selectMany)
        {
            var t = CreateTable(typeof(T), string.Empty);
            CreateColumnsForType(t, typeof(T), string.Empty);

            foreach (var item in selectMany) CreateRowForObject(t, item, string.Empty);
        }

        public void AddItems<T>(IEnumerable<T> selectMany, string tableName)
        {
            var t = CreateTable(typeof(T), tableName);
            CreateColumnsForType(t, typeof(T), string.Empty);

            foreach (var item in selectMany) CreateRowForObject(t, item, string.Empty);
        }


        /// <summary>
        ///     Eine Benutzerdefinierte Tabelle einfügen
        ///     Es muss die TableDef aufgerufen werden zur weiteren definition
        /// </summary>
        /// <param name="tabledef"></param>
        public void CreateCustomTable(Action<CustomTableDefinition> tabledef)
        {
            var def = new CustomTableDefinition();
            tabledef(def);

            var table = CreateTable(null, def.TableName);

            foreach (var type in def.Types.Keys)
            {
                var pr = def.Types[type];
                table.CreateColumnsForType(type, pr);
            }

            foreach (var values in def.ValuesList)
            foreach (var o in values)
            {
                var pr = def.Types[o.GetType()];
                table.CreateRowForObject(o, pr);
            }
        }

        /// <summary>
        ///     Eine Tabelle für ein Objekt erstellen und
        ///     Definiition aufrufen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tabledef"></param>
        public void CreateObjectTable<T>(Action<ObjectTableDefinition<T>> tabledef)
        {
            var def = new ObjectTableDefinition<T>();
            tabledef(def);

            var table = CreateTable(typeof(T), def.TableName);
            table.CreateColumnsForType(typeof(T), string.Empty);

            foreach (var o in def.Items) table.CreateRowForObject(o);
        }

        /// <summary>
        ///     Die Daten in eine Dateiexportieren
        ///     DateiName  DataSetName_File.xml"
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        public void ExportData(string folder, string file)
        {
            var fileName = Path.Combine(folder, string.Format("{0}_{1}.xml", DataSet.DataSetName, file));
            DataSet.WriteXml(fileName);
        }

        /// <summary>
        ///     Das Schema in einen Ordner exportieren
        ///     Als Dateiname wird {0}_Schema.xsd gewählt
        /// </summary>
        /// <param name="folder"></param>
        public void ExportSchema(string folder)
        {
            var fileName = Path.Combine(folder, string.Format("{0}_Schema.xsd", DataSet.DataSetName));
            DataSet.WriteXmlSchema(fileName);
        }

        private static void CreateColumnsForType(DataTable table, Type objectType, string prefix)
        {
            foreach (var p in GetProps(objectType))
            {
                string cName;
                if (!string.IsNullOrEmpty(prefix))
                {
                    cName = prefix + "_" + p.Name;
                }
                else
                {
                    cName = p.Name;
                }

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
                    {
                        ctype = typeof(string);
                    }
                }

                table.Columns.Add(cName, ctype ?? throw new InvalidOperationException());
            }
        }

        private static void CreateRowForObject(DataTable table, object objectWithValue, string prefix)
        {
            if (objectWithValue == null)
            {
                return;
            }

            var objectType = objectWithValue.GetType();

            var row = table.NewRow();

            foreach (var p in GetProps(objectType))
            {
                string cName;
                if (!string.IsNullOrEmpty(prefix))
                {
                    cName = prefix + "_" + p.Name;
                }
                else
                {
                    cName = p.Name;
                }


                //wenn enum, dann tostring, sonst
                var v = p.GetValue(objectWithValue, null);
                if (v == null)
                {
                    row[cName] = DBNull.Value;
                }
                else
                {
                    row[cName] = v;
                }
            }

            table.Rows.Add(row);
        }

        private DataTable CreateTable(Type t, string forceTableName)
        {
            var tableName = t.Name;

            //if (t != null)
            //{
            //    tableName = t.Name;
            //    var tattr = t.GetCustomAttributes(true).OfType<TableAttribute>().FirstOrDefault();
            //    if (tattr != null)
            //        tableName = tattr.Name;
            //}

            if (!string.IsNullOrEmpty(forceTableName))
            {
                tableName = forceTableName;
            }

            return DataSet.Tables.Add(tableName);
        }

        private static IEnumerable<PropertyInfo> GetProps(Type t)
        {
            var list = new List<PropertyInfo>();

            //alle properties auslesen
            foreach (var p in t.GetProperties())
            {
                if (p.PropertyType.Namespace != null &&
                    !p.PropertyType.IsArray &&
                    (p.PropertyType.IsPublic && p.PropertyType.Namespace.ToLower() == "system" ||
                     p.PropertyType.IsEnum))
                {
                    list.Add(p);
                }

                if (p.PropertyType == typeof(byte[]))
                {
                    list.Add(p);
                }

                //if (p.PropertyType.BaseType.FullName.ToLower() == "system.enum" && p.PropertyType.IsPublic)
                //{
                //  list.RegisterEffect(p);
                //}
            }

            return list.OrderBy(x => x.Name);
        }

        /// <summary>
        ///     Definition einer Benutzerdefinierten Tabelle
        /// </summary>
        public class CustomTableDefinition
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="CustomTableDefinition" /> class.
            /// </summary>
            public CustomTableDefinition()
            {
                Types = new Dictionary<Type, string>();
                ValuesList = new List<object[]>();
            }

            /// <summary>
            ///     Name der Tabelle
            /// </summary>
            public string TableName { get; private set; }

            /// <summary>
            ///     Typen, die enthalten sind, mit den Präfixen
            /// </summary>
            public Dictionary<Type, string> Types { get; }

            /// <summary>
            ///     Liste der Werte, ein Array pro Zeile mit verschiedenen objekten
            /// </summary>
            public List<object[]> ValuesList { get; }

            /// <summary>
            ///     Fügt eine Row in die Table ein, eine Liste von Objekten muss übergeben werden
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public CustomTableDefinition AddRow(params object[] values)
            {
                ValuesList.Add(values);
                return this;
            }

            /// <summary>
            ///     Fügbt eine Liste von Rows ein.
            /// </summary>
            /// <param name="insertAction"></param>
            /// <returns></returns>
            public CustomTableDefinition AddRows(Action<IList<object[]>> insertAction)
            {
                insertAction(ValuesList);
                return this;
            }

            /// <summary>
            ///     Erzeugt die Spalten für einen Typ und setzt das Prefix
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="typePrefix"></param>
            /// <returns></returns>
            public CustomTableDefinition CreateColumnsForType<T>(string typePrefix)
            {
                Types.Add(typeof(T), typePrefix);
                return this;
            }

            /// <summary>
            ///     Erzeugt die Spalten für einen Typ ohne Prefix
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public CustomTableDefinition CreateColumnsForType<T>()
            {
                return CreateColumnsForType<T>(typeof(T).Name);
            }

            /// <summary>
            ///     Definiert den TableName
            /// </summary>
            /// <param name="tableNam"></param>
            /// <returns></returns>
            public CustomTableDefinition WithName(string tableNam)
            {
                TableName = tableNam;
                return this;
            }
        }

        /// <summary>
        ///     Table für ein einzelnen Objekt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ObjectTableDefinition<T>
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="ObjectTableDefinition{T}" /> class.
            /// </summary>
            public ObjectTableDefinition()
            {
                Items = new List<T>();
            }

            /// <summary>
            ///     Rows, Liste der Objekte
            /// </summary>
            public List<T> Items { get; }

            /// <summary>
            ///     Name der Tabelle
            /// </summary>
            public string TableName { get; private set; }

            /// <summary>
            ///     Ein element einfügen
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public ObjectTableDefinition<T> AddItem(T item)
            {
                Items.Add(item);
                return this;
            }

            /// <summary>
            ///     Mehrere Elemente einfügen
            /// </summary>
            /// <param name="items"></param>
            /// <returns></returns>
            public ObjectTableDefinition<T> AddItems(IEnumerable<T> items)
            {
                Items.AddRange(items);
                return this;
            }

            /// <summary>
            ///     Table Name vergeben
            /// </summary>
            /// <param name="tableNam"></param>
            /// <returns></returns>
            public ObjectTableDefinition<T> WithName(string tableNam)
            {
                TableName = tableNam;
                return this;
            }
        }
    }
}