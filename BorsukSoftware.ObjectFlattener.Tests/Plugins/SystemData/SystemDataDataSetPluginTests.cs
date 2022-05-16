using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    /// <summary>
    /// Tests for <see cref="SystemDataDataSetPlugin"/>
    /// </summary>
    public class SystemDataDataSetPluginTests
    {
        [InlineData(null)]
        [InlineData("bob")]
        [Theory]
        public void ByIndex(string prefix)
        {
            this.Common(prefix, DataSetTableNameMode.Index, (idx, _) => $"[{idx}]");
        }

        [InlineData(null)]
        [InlineData("bob")]
        [Theory]
        public void ByName(string prefix)
        {
            this.Common(prefix, DataSetTableNameMode.Name, (_, dt) => $"[{dt.TableName}]");
        }

        [InlineData(null)]
        [InlineData("bob")]
        [Theory]
        public void Custom(string prefix)
        {
            this.Common(prefix, DataSetTableNameMode.Custom, (idx, dt) => $"[{idx}-{dt.TableName}]");
        }

        private void Common(string prefix, DataSetTableNameMode dataSetTableNameMode, Func<int, System.Data.DataTable, string> nameFunc)
        {
            var dt1 = new System.Data.DataTable();
            dt1.TableName = "DT1";
            dt1.Columns.Add("Col 1", typeof(string));
            dt1.Columns.Add("Col 2", typeof(string));
            dt1.Rows.Add(new object[] { "val1_1", "val1_2" });
            dt1.Rows.Add(new object[] { "val2_1", "val2_2" });
            dt1.Rows.Add(new object[] { "val3_1", "val3_2" });

            var dt2 = new System.Data.DataTable();
            dt2.TableName = "DT2";
            dt2.Columns.Add("Col 1", typeof(string));
            dt2.Columns.Add("Col 2", typeof(string));
            dt2.Columns.Add("Col 3", typeof(string));

            dt2.Rows.Add(new object[] { "val1_1", "val1_2", "val1_3" });
            dt2.Rows.Add(new object[] { "val2_1", "val2_2", "val2_3" });

            var ds = new System.Data.DataSet();
            ds.Tables.Add(dt1);
            ds.Tables.Add(dt2);

            var objectFlattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw
            };
            objectFlattener.Plugins.Add(new SystemDataDataSetPlugin()
            {
                DataSetTableNameMode = dataSetTableNameMode,
                DataSetTableNameCustomFunc = nameFunc
            });

            var dtPlugin = new SystemDataDataTablePlugin();
            objectFlattener.Plugins.Add(dtPlugin);
            objectFlattener.Plugins.Add(new StandardPlugin());

            var values = objectFlattener.FlattenObject(prefix, ds).ToList();

            var dts = new[] { dt1, dt2 };
            var expectedValues = dts.
                Select((dt, idx) => new { dt, adjustedPrefix = $"{prefix}{nameFunc(idx, dt)}" }).
                SelectMany( tuple => objectFlattener.FlattenObject( tuple.adjustedPrefix, tuple.dt ));

            values.Should().BeEquivalentTo(expectedValues);
        }
    }
}