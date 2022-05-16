namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    public enum ColumnNamingMode
    {
        /// <summary>
        /// The field will be [{columnName}]
        /// </summary>
        ColumnName,

        /// <summary>
        /// The field will be [{colIndex}]
        /// </summary>
        Index,

        /// <summary>
        /// Custom choice
        /// </summary>
        Custom
    }
}
