using System;
using System.Data;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using SoftwarePioniere.Util;
using Xunit;
using Xunit.Abstractions;
using SoftwarePioniere.ReadModel;
using DataSetExtensions = SoftwarePioniere.ReadModel.DataSetExtensions;

namespace SoftwarePioniere.Tests.ReadModel
{
    public class ReportingExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public ReportingExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private DataSet CreateDataSet()
        {
            var helper = new ReportDataHelper("c1");
            helper.AddItems(new[]{ new Class1
            {
                MyTimeUtc = DateTime.UtcNow,
                Text1 = "aa"
            },new Class1
            {
                MyTimeUtc = DateTime.UtcNow.AddDays(1),
                Text1 = "bb"
            }});

            var ds = helper.DataSet;

            return ds;
        }

        [Fact]
        public void SerilizationTests()
        {
            var ds = CreateDataSet();
          
            ds.Should().NotBeNull();
            ds.Tables.Count.Should().Be(1);
            ds.Tables[0].Rows.Count.Should().Be(2);

            var dataDoc = ds.GetDataXDocument();
            var dataJson = JsonConvert.SerializeXNode(dataDoc, Formatting.None);
            dataJson.Should().NotBeNullOrEmpty();
            _output.WriteLine(dataJson);

            dataJson = ds.GetDataJson();
            dataJson.Should().NotBeNullOrEmpty();
            _output.WriteLine(dataJson);

            var schemaDoc = ds.GetSchemaXDocument();
            var schemaJson = JsonConvert.SerializeXNode(schemaDoc, Formatting.None);
            schemaJson.Should().NotBeNullOrEmpty();
            _output.WriteLine(schemaJson);
            
            schemaJson = ds.GetSchemaJson();
            schemaJson.Should().NotBeNullOrEmpty();
            _output.WriteLine(schemaJson);

            dataDoc = JsonConvert.DeserializeXNode(dataJson);
            dataDoc.Should().NotBeNull();

            schemaDoc = JsonConvert.DeserializeXNode(schemaJson);
            schemaDoc.Should().NotBeNull();

            ds = dataDoc.GetDataSet(schemaDoc);
            ds.Should().NotBeNull();
            ds.Tables.Count.Should().Be(1);
            ds.Tables[0].Rows.Count.Should().Be(2);

            ds = DataSetExtensions.CreateDataSet(dataJson, schemaJson);
            ds.Should().NotBeNull();
            ds.Tables.Count.Should().Be(1);
            ds.Tables[0].Rows.Count.Should().Be(2);

        }


        [Fact]
        public void SerilizationTests2()
        {
            var ds = CreateDataSet();
            ds.Tables.Count.Should().Be(1);
            ds.Tables[0].Rows.Count.Should().Be(2);

            var dataJson = ds.GetDataJson();
            dataJson.Should().NotBeNullOrEmpty();
            _output.WriteLine(dataJson);


        }

        [Fact]
        public void CreateLocalDateTimesTest()
        {
            var helper = new ReportDataHelper("c1");
            helper.Add(new Class1
            {
                MyTimeUtc = DateTime.UtcNow,
                Text1 = "aa"
            });


            var tz = TimeZoneInfo.Local;
            tz.Should().NotBeNull();

            var ds = helper.DataSet;
            ds.Should().NotBeNull();
            ds.Tables.Count.Should().Be(1);
            ds.Tables[0].Rows.Count.Should().Be(1);

            ds.CreateLocalDateTimes(tz);

            ds.Tables[0].Columns.Cast<DataColumn>().Select(x => x.ColumnName).Should().Contain("MyTime");


        }

        private class Class1
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DateTime MyTimeUtc { get; set; }
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Text1 { get; set; }
        }
    }
}
