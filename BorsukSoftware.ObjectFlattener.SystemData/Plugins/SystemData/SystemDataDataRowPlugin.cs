using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    /// <summary>
    /// Standard plugin for handling <see cref="System.Data.DataRow"/> objects
    /// </summary>
    /// <remarks>In general, the output will be of the format [col]. The exact format depends on the configuration.</remarks>
    public class SystemDataDataRowPlugin : IObjectFlatteningPlugin
    {
        #region Data Model

        /// <summary>
        /// Get / set how the column portion of the address should be generated
        /// </summary>
        public ColumnNamingMode ColumnNamingMode { get; set; } = ColumnNamingMode.ColumnName;

        /// <summary>
        /// Get / set the custom func to use when <see cref="ColumnNamingMode"/> is equal to <see cref="SystemData.ColumnNamingMode.Custom"/>
        /// </summary>
        public Func<int, System.Data.DataColumn, string> ColumnNamingCustomFunc { get; set; }

        /// <summary>
        /// Get / set the behaviour when an instance of <see cref="DBNull"/> is found
        /// </summary>
        public DBNullHandlingMode DBNullHandlingMode { get; set; } = DBNullHandlingMode.Skip;

        #endregion

        public bool CanHandle(string prefix, object @object)
        {
            return @object is System.Data.DataRow;
        }

        public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
        {
            var row = @object as System.Data.DataRow;
            if (row == null)
                throw new InvalidOperationException($"Cannot flatten non DataRow objects - {@object}");

            Func<int, System.Data.DataColumn, string> colNamingFunc;
            switch (this.ColumnNamingMode)
            {
                case ColumnNamingMode.ColumnName:
                    colNamingFunc = (index, column) =>
                    {
                        var columnName = column.ColumnName;
                        bool quotesRequired = columnName.Contains("]") || columnName.Contains("[");
                        if (quotesRequired)
                            return $"[\"{columnName}\"]";
                        else
                            return $"[{columnName}]";
                    };
                    break;

                case ColumnNamingMode.Index:
                    colNamingFunc = (index, _) => $"[{index}]";
                    break;

                case ColumnNamingMode.Custom:
                    if (this.ColumnNamingCustomFunc == null)
                        throw new InvalidOperationException($"Custom col naming mode specified, but no custom func was specified");

                    colNamingFunc = this.ColumnNamingCustomFunc;
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported col naming mode - {this.ColumnNamingMode}");
            }

            var columnNames = new List<string>(row.Table.Columns.Count);
            for (int i = 0; i < row.Table.Columns.Count; i++)
                columnNames.Add(colNamingFunc(i, row.Table.Columns[i]));

            // Check for duplicates
            {
                var columnNamesAsSet = new HashSet<string>();
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (!columnNamesAsSet.Add(columnNames[i]))
                        throw new InvalidOperationException($"Duplicate column name '{columnNames[i]}' found (2nd index = #{i}");
                }
            }


            for (int colIdx = 0; colIdx < row.Table.Columns.Count; colIdx++)
            {
                var value = row[colIdx];
                bool returnValue = false;
                if (value is DBNull)
                {
                    switch (this.DBNullHandlingMode)
                    {
                        case DBNullHandlingMode.ReturnDBNull:
                            returnValue = true;
                            break;

                        case DBNullHandlingMode.ReturnNull:
                            returnValue = true;
                            value = null;
                            break;

                        case DBNullHandlingMode.Skip:
                            continue;

                        default:
                            throw new InvalidOperationException($"Found DBNull instance, but invalid handling mode - {this.DBNullHandlingMode}");
                    }
                }

                var colPortion = columnNames[colIdx];
                var newName = $"{prefix}{colPortion}";

                if (returnValue)
                    yield return new KeyValuePair<string, object>(newName, value);
                else
                    foreach (var valuePair in objectFlattener.FlattenObject(newName, value))
                        yield return valuePair;
            }
        }
    }
}