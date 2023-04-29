using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using System.Data;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    /// <summary>
    /// Tests for <see cref="SystemDataDataRowPlugin"/>
    /// </summary>
    public class SystemDataDataRowPluginTests
    {
        #region Duplicate column names

        [Fact]
        public void CheckRowAsNestedObject()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });

            var testObject = new
            {
                myRow = dt.Rows[0]
            };

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
            });
            flattener.Plugins.Add(new StandardPlugin());

            string prefix = null;
            var flattenedRepresentation = flattener.FlattenObject(prefix, testObject).ToList();

            flattenedRepresentation.Should().BeEquivalentTo(new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>( "myRow[0]", "val1"),
                new KeyValuePair<string, object>( "myRow[1]", 2),
                new KeyValuePair<string, object>( "myRow[2]", 2.3),
            });
        }

        #endregion


        #region Duplicate column names

        [Fact]
        public void DuplicateColumnName_Throws()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Custom,
                ColumnNamingCustomFunc = (_, _) => "Clash",
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "row";

            Assert.Throws<InvalidOperationException>(() => flattener.FlattenObject(prefix, dt.Rows[0]).ToList());
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

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dt.Rows[0]).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[1]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[2]", 2.3 ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_Custom()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Custom,
                ColumnNamingCustomFunc = (idx, col) => $"[Bobble-{idx}-{col}]",
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dt.Rows[0]).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[Bobble-0-Col 1]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[Bobble-1-Col 2]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[Bobble-2-Col 3]", 2.3 ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_Name()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Columns.Add("Col 2", typeof(int));
            dt.Columns.Add("Col 3]", typeof(double));

            dt.Rows.Add(new object[] { "val1", 2, 2.3 });

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.ColumnName,
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dt.Rows[0]).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[Col 1]", "val1" ),
                new KeyValuePair<string, object>( $"{prefix}[Col 2]", 2 ),
                new KeyValuePair<string, object>( $"{prefix}[\"Col 3]\"]", 2.3 ),
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

            dt.Rows.Add(new object[] { DBNull.Value });

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                DBNullHandlingMode = DBNullHandlingMode.Skip
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dt.Rows[0]).ToList();
            Assert.Empty(extractedValues);
        }

        [Fact]
        public void DBNullChecks_Null()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Rows.Add(new object[] { DBNull.Value });

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                DBNullHandlingMode = DBNullHandlingMode.ReturnNull
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "table";
            var extractedValues = flattener.FlattenObject(prefix, dt.Rows[0]).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0]", null ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void DBNullChecks_DBNull()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col 1", typeof(string));
            dt.Rows.Add(new object[] { DBNull.Value });

            var flattener = new ObjectFlattener()
            {
                NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw,
            };

            flattener.Plugins.Add(new SystemDataDataRowPlugin()
            {
                ColumnNamingMode = ColumnNamingMode.Index,
                DBNullHandlingMode = DBNullHandlingMode.ReturnDBNull
            });
            flattener.Plugins.Add(new StandardPlugin());

            var prefix = "row";
            var extractedValues = flattener.FlattenObject(prefix, dt.Rows[0]).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( $"{prefix}[0]", DBNull.Value ),
            };

            extractedValues.Should().BeEquivalentTo(expectedValues);
        }

        #endregion
    }
}
