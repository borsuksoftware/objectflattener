namespace BorsukSoftware.ObjectFlattener.Plugins.SystemXml
{
    /// <summary>
    /// Enum detailing how the 
    /// </summary>
    public enum NodeNamingMode
    {
        /// <summary>
        /// The plugin will manage the naming of all elements according to standard rules
        /// </summary>
        Standard,

        /// <summary>
        /// The plugin will defer to an externally provided function to generate the name of the node, for all support node types (e.g. elements, attributes, text etc.)
        /// </summary>
        CustomFunc,
    }
}
