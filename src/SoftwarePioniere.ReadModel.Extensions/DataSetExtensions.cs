using System.Data;
using Newtonsoft.Json;
using SoftwarePioniere.Util;

namespace SoftwarePioniere.ReadModel
{
    public static class DataSetExtensions
    {

        public static string GetDataJson(this DataSet dataSet)
        {
            var doc = dataSet.GetDataXDocument();
            var json = JsonConvert.SerializeXNode(doc, Formatting.None);
            return json;
        }

        public static string GetSchemaJson(this DataSet dataSet)
        {
            var doc = dataSet.GetSchemaXDocument();
            var json = JsonConvert.SerializeXNode(doc, Formatting.None);
            return json;
        }

        public static DataSet CreateDataSet(string dataJson, string schemaJson)
        {
            var dataDoc = JsonConvert.DeserializeXNode(dataJson);
            var schemaDoc = JsonConvert.DeserializeXNode(schemaJson);

            var ds = dataDoc.GetDataSet(schemaDoc);

            return ds;
        }

    }
}
