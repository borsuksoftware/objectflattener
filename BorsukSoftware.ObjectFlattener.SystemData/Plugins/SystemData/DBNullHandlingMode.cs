namespace BorsukSoftware.ObjectFlattener.Plugins.SystemData
{
    public enum DBNullHandlingMode
    {
        /// <summary>
        /// When a DB null is seen, the cell should be skipped
        /// </summary>
        Skip,

        /// <summary>
        /// When a DB null is seen, it should be processed as a null
        /// </summary>
        ReturnNull,

        /// <summary>
        /// When a DB null is seen, it should be returned
        /// </summary>
        ReturnDBNull,
    }
}
