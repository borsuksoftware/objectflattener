using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    /// <summary>
    /// Standard plugin to flatten instances of <see cref="System.Data.DataSet"/>
    /// </summary>
    public class SystemDataDataSetPlugin : IObjectFlatteningPlugin
    {
        #region Data Model

        public DataSetTableNameMode DataSetTableNameMode { get; set; } = DataSetTableNameMode.Name;
        public Func<int, System.Data.DataTable, string> DataSetTableNameCustomFunc { get; set; }

        #endregion

        #region IObjectFlatteningPlugin Members

        public bool CanHandle(string prefix, object @object)
        {
            return @object is System.Data.DataSet;
        }

        public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
        {
            var ds = @object as System.Data.DataSet;
            if (ds == null)
                throw new InvalidOperationException($"This plugin can only process DataSet objects - {@object}");

            bool checkDuplicates = true;
            Func<int, System.Data.DataTable, string> tableNameFunc;
            switch (this.DataSetTableNameMode)
            {
                case DataSetTableNameMode.Index:
                    tableNameFunc = (i, _) => $"[{i}]";
                    checkDuplicates = false;
                    break;

                case DataSetTableNameMode.Name:
                    tableNameFunc = (_, table) =>
                    {
                        if (string.IsNullOrEmpty(table.TableName))
                            throw new InvalidOperationException($"DataSetTableNameMode of name specified, but table has no name");

                        var name = table.TableName;
                        bool quotesRequired = name.Contains("[") || name.Contains("]");

                        if (quotesRequired)
                            return $"[\"{name}\"]";
                        else
                            return $"[{name}]";
                    };
                    break;

                case DataSetTableNameMode.Custom:
                    if (this.DataSetTableNameCustomFunc == null)
                        throw new InvalidOperationException($"Custom mode specified, but no custom func specified");

                    tableNameFunc = this.DataSetTableNameCustomFunc;
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported table name mode specified - {this.DataSetTableNameMode}");
            }

            // Check for name duplication
            if( checkDuplicates)
            {
                var existingNames = new HashSet<string>();
                for (int tableIdx = 0; tableIdx < ds.Tables.Count; tableIdx++)
                {
                    var candidateName = tableNameFunc(tableIdx, ds.Tables[tableIdx]);

                    if (!existingNames.Add(candidateName))
                        throw new InvalidOperationException($"Duplicate name '{candidateName}' found for table (duplicate idx = {tableIdx})");
                }
            }

            for (int tableIdx = 0; tableIdx < ds.Tables.Count; tableIdx++)
            {
                var table = ds.Tables[tableIdx];

                var name = tableNameFunc(tableIdx, table);

                var newName = $"{prefix}{name}";
                foreach (var value in objectFlattener.FlattenObject(newName, table))
                    yield return value;
            }
        }

        #endregion
    }
}
