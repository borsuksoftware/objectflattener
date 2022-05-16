using System;
using System.Collections.Generic;
using System.Linq;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    /// <summary>
    /// Standard plugin for handling <see cref="System.Data.DataView"/> objects
    /// </summary>
    /// <remarks>In general, the output will be of the format [row][col]. The exact format depends on the configuration.</remarks>
    public class SystemDataDataViewPlugin : IObjectFlatteningPlugin
    {
        #region Data Model

        /// <summary>
        /// Get / set how the row portion of the address should be generated
        /// </summary>
        public RowNamingMode RowNamingMode { get; set; } = RowNamingMode.RowIndex;

        /// <summary>
        /// Get / set the custom func to use when <see cref="RowNamingMode"/> is equal to <see cref="SystemData.RowNamingMode.Custom"/>
        /// </summary>
        public Func<int, System.Data.DataRowView, string> RowNamingCustomFunc { get; set; }

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
            return @object is System.Data.DataView;
        }

        public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
        {
            var dv = @object as System.Data.DataView;
            if (dv == null)
                throw new InvalidOperationException($"Cannot flatten non DataView objects - {@object}");

            Func<int, System.Data.DataRowView, string> rowNamingFunc;
            switch (this.RowNamingMode)
            {
                case RowNamingMode.Custom:
                    if (this.RowNamingCustomFunc == null)
                        throw new InvalidOperationException($"Custom row naming mode specified, but no custom func was specified");

                    rowNamingFunc = this.RowNamingCustomFunc;
                    break;

                case RowNamingMode.RowIndex:
                    rowNamingFunc = (index, _) => $"[{index}]";
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported row naming mode - {this.RowNamingMode}");
            }

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

            var columnNames = new List<string>(dv.Table.Columns.Count);
            for (int i = 0; i < dv.Table.Columns.Count; i++)
                columnNames.Add(colNamingFunc(i, dv.Table.Columns[i]));

            // Check for duplicates
            {
                var columnNamesAsSet = new HashSet<string>();
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (!columnNamesAsSet.Add(columnNames[i]))
                        throw new InvalidOperationException($"Duplicate column name '{columnNames[i]}' found (2nd index = #{i}");
                }
            }

            var rows = dv.Cast<System.Data.DataRowView>().Select((row, idx) => new { row, idx });
            foreach( var rowTuple in rows)
            {
                var rowIdx = rowTuple.idx;
                var row = rowTuple.row;
                for (int colIdx = 0; colIdx < dv.Table.Columns.Count; colIdx++)
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

                    var rowPortion = rowNamingFunc(rowIdx, row);
                    var colPortion = columnNames[colIdx];
                    var newName = $"{prefix}{rowPortion}{colPortion}";

                    if (returnValue)
                        yield return new KeyValuePair<string, object>(newName, value);
                    else
                        foreach (var valuePair in objectFlattener.FlattenObject(newName, value))
                            yield return valuePair;
                }
            }
        }
    }
}
