using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    /// <summary>
    /// Tests for <see cref="SystemDataDataViewPlugin"/>
    /// </summary>
    public class SystemDataDataViewPluginTests
    {
        #region Duplicate column names

        [Fact]
        public void DuplicateColumnName_Throws()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });
            dt.Rows.Add(new object[] { "val2", 6, 1.3 });
            dt.Rows.Add(new object[] { "val3", 7, 2.1 });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'val2'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Custom,
                ColumnNamingCustomFunc = (_, _) => "Clash",
                RowNamingMode = RowNamingMode.RowIndex,
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";

            Assert.Throws<InvalidOperationException>(() => flattener.FlattenObject(prefix, dv).ToList());
        }

        #endregion

        #region Row Options

        [Fact]
        public void Standard_Custom_Index()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });
            dt.Rows.Add(new object[] { "val2", 6, 1.3 });
            dt.Rows.Add(new object[] { "val3", 7, 2.1 });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'val2'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                RowNamingMode = RowNamingMode.Custom,
                RowNamingCustomFunc = (idx, row) => $"[boggle-{idx}]",
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[boggle-0][0]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[boggle-0][1]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[boggle-0][2]", 2.3 ),
                new KeyValuePair<string, object>( $"{prefix}[boggle-1][0]", "val3" ),
                new KeyValuePair<string, object>( $"{prefix}[boggle-1][1]", 7 ),
                new KeyValuePair<string, object>( $"{prefix}[boggle-1][2]", 2.1 ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        #endregion

        #region Column Options

        [Fact]
        public void Standard_Index_Index()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });
            dt.Rows.Add(new object[] { "val2", 6, 1.3 });
            dt.Rows.Add(new object[] { "val3", 7, 2.1 });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'val2'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                RowNamingMode = RowNamingMode.RowIndex
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0][0]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[0][1]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[0][2]", 2.3 ),
                new KeyValuePair<string, object>( $"{prefix}[1][0]", "val3" ),
                new KeyValuePair<string, object>( $"{prefix}[1][1]", 7 ),
                new KeyValuePair<string, object>( $"{prefix}[1][2]", 2.1 ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_Index_Custom()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });
            dt.Rows.Add(new object[] { "val2", 6, 1.3 });
            dt.Rows.Add(new object[] { "val3", 7, 2.1 });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'val2'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Custom,
                ColumnNamingCustomFunc = (idx, col) => $"[Bobble-{idx}-{col}]",
                RowNamingMode = RowNamingMode.RowIndex
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0][Bobble-0-Col 1]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[0][Bobble-1-Col 2]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[0][Bobble-2-Col 3]", 2.3 ),
                new KeyValuePair<string, object>( $"{prefix}[1][Bobble-0-Col 1]", "val3" ),
                new KeyValuePair<string, object>( $"{prefix}[1][Bobble-1-Col 2]", 7 ),
                new KeyValuePair<string, object>( $"{prefix}[1][Bobble-2-Col 3]", 2.1 ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_Index_Name()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3]", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });
            dt.Rows.Add(new object[] { "val2", 6, 1.3 });
            dt.Rows.Add(new object[] { "val3", 7, 2.1 });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'val2'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.ColumnName,
                RowNamingMode = RowNamingMode.RowIndex
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0][Col 1]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[0][Col 2]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[0][\"Col 3]\"]", 2.3 ),
                new KeyValuePair<string, object>( $"{prefix}[1][Col 1]", "val3" ),
                new KeyValuePair<string, object>( $"{prefix}[1][Col 2]", 7 ),
                new KeyValuePair<string, object>( $"{prefix}[1][\"Col 3]\"]", 2.1 ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }
        #endregion

        #region DBNull Checks

        [Fact]
        public void DBNullChecks_Skip()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(string));

            dt.Rows.Add(new object[] { "key1", "value1" });
            dt.Rows.Add(new object[] { "key2", DBNull.Value });
            dt.Rows.Add(new object[] { "key3", DBNull.Value });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'key3'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                RowNamingMode = RowNamingMode.RowIndex,
                DBNullHandlingMode = DBNullHandlingMode.Skip
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0][0]", "key1" ),
                new KeyValuePair<string, object>( $"{prefix}[0][1]", "value1" ),
                new KeyValuePair<string, object>( $"{prefix}[1][0]", "key2" ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void DBNullChecks_Null()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(string));

            dt.Rows.Add(new object[] { "key1", "value1" });
            dt.Rows.Add(new object[] { "key2", DBNull.Value });
            dt.Rows.Add(new object[] { "key3", DBNull.Value });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'key3'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                RowNamingMode = RowNamingMode.RowIndex,
                DBNullHandlingMode = DBNullHandlingMode.ReturnNull
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0][0]", "key1" ),
                new KeyValuePair<string, object>( $"{prefix}[0][1]", "value1" ),
                new KeyValuePair<string, object>( $"{prefix}[1][0]", "key2" ),
                new KeyValuePair<string, object>( $"{prefix}[1][1]", null )
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void DBNullChecks_DBNull()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(string));

            dt.Rows.Add(new object[] { "key1", "value1" });
            dt.Rows.Add(new object[] { "key2", DBNull.Value });
            dt.Rows.Add(new object[] { "key3", DBNull.Value });

            var dv = new System.Data.DataView(dt, "[Col 1] <> 'key3'", null, System.Data.DataViewRowState.CurrentRows);

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataViewPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                RowNamingMode = RowNamingMode.RowIndex,
                DBNullHandlingMode = DBNullHandlingMode.ReturnDBNull
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dv).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0][0]", "key1" ),
                new KeyValuePair<string, object>( $"{prefix}[0][1]", "value1" ),
                new KeyValuePair<string, object>( $"{prefix}[1][0]", "key2" ),
                new KeyValuePair<string, object>( $"{prefix}[1][1]", DBNull.Value )
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        #endregion
    }
}
