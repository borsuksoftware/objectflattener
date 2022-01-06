namespace BorsukSoftware.ObjectFlattener.Plugins.SystemXml
{
    /// <summary>
    /// Enum detailing how leaf nodes should be treated within the framework
    /// </summary>
    public enum LeafNodeReportingBehaviour
    {
        /// <summary>
        /// Leaf nodes are treated as any other node, explicitly their attributes will be reported, but nothing else
        /// </summary>
        None,

        /// <summary>
        /// Reports an empty string for leaf nodes with no attributes where the node was defined as &lt;myNode&gt;&lt;/myNode&gt; but not &lt;/ myNode&gt;
        /// </summary>
        ReportEmptyStringIfNoAttributesAndNodeIsNotEmpty,

        /// <summary>
        /// Reports an empty string for leaf with no attributes
        /// </summary>
        ReportEmptyStringIfNoAttributes,

        /// <summary>
        /// Reports an empty string for all leaf nodes, regardless of whether or not they have attributes or if they were marked as empty
        /// </summary>
        AlwaysReportEmptyString,
    }
}
