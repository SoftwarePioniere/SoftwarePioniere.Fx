using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoftwarePioniere.Util
{
    public static class DataSetExtensions
    {
        //    public static string GetJson(this XDocument doc)
        //    {

        //    }


        public static XDocument GetDataXDocument(this DataSet dataSet)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) { Formatting = Formatting.None })
                {
                    dataSet.WriteXml(xmlTextWriter);
                    memoryStream.Position = 0;
                    var xmlReader = XmlReader.Create(memoryStream);
                    xmlReader.MoveToContent();
                    return XDocument.Load(xmlReader);
                }
            }
        }

        public static XDocument GetSchemaXDocument(this DataSet dataSet)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) { Formatting = Formatting.None })
                {
                    dataSet.WriteXmlSchema(xmlTextWriter);
                    memoryStream.Position = 0;
                    var xmlReader = XmlReader.Create(memoryStream);
                    xmlReader.MoveToContent();
                    return XDocument.Load(xmlReader);
                }
            }
        }

        public static DataSet GetDataSet(this XDocument data, XDocument schema = null)
        {
            var ds = new DataSet();

            {
                var xmlReader = data.CreateReader(ReaderOptions.None);
                xmlReader.MoveToContent();
                ds.ReadXml(xmlReader);

            }

            if (schema != null)
            {
                var xmlReader = schema.CreateReader(ReaderOptions.None);
                xmlReader.MoveToContent();
                ds.ReadXmlSchema(xmlReader);
            }

            return ds;
        }

        [Obsolete]
        public static string GetXmlSchemaWithUtf8(this DataSet dataSet)
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(new StringWriterWithEncoding(sb, Encoding.UTF8)))
            {
                dataSet.WriteXmlSchema(writer);
            }
            return sb.ToString();
        }

        [Obsolete]
        public static string GetXmlDataWithUtf8(this DataSet dataSet)
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(new StringWriterWithEncoding(sb, Encoding.UTF8)))
            {
                dataSet.WriteXml(writer);
            }
            return sb.ToString();
        }

        public static void CreateLocalDateTimes(this DataSet ds, TimeZoneInfo tz)
        {
            //var tz = TimeZoneUtil.GetHomeTimeZone();

            foreach (DataTable table in ds.Tables)
            {
                var cols = new Dictionary<string, string>();

                foreach (DataColumn column in table.Columns)
                {
                    if (column.DataType == typeof(DateTime) && column.ColumnName.ToLower().Contains("utc"))
                    {
                        cols.Add(column.ColumnName, column.ColumnName.Replace("Utc", ""));
                    }
                }

                if (cols.Keys.Count > 0)
                {
                    foreach (var key in cols.Keys)
                    {
                        table.Columns.Add(cols[key], typeof(DateTime));
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (var key in cols.Keys)
                        {
                            if (!row.IsNull(key))
                            {
                                var utcValue = (DateTime)row[key];
                                var utcDate = TimeZoneInfo.ConvertTimeToUtc(utcValue, TimeZoneInfo.Utc);

                                var localValue = TimeZoneInfo.ConvertTime(utcDate, TimeZoneInfo.Utc, tz);
                                row[cols[key]] = localValue;

                            }
                        }
                    }
                }
            }
        }

    }
}