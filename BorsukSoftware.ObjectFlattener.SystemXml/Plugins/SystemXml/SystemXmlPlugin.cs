using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemXml
{
    /// <summary>
    /// Standard implementation of <see cref="IObjectFlatteningPlugin"/> to handle objects within the System.Xml namespace
    /// </summary>
    /// <remarks>
    /// <para>The plugin is designed to make it easy to take a hierarchical XML document / node and represent it as a flattened
    /// set of key-value pairs. There are various configuration options within the plugin to determine how to handle empty nodes etc.</para>
    /// 
    /// <para>Given that XML allows for multiple child nodes with the same name, the default naming convention used cannot be guaranteed
    /// to return a uniquely named set of properties. There are various flags to allow this (by appending the index after clashes) or custom
    /// naming mode can be used. The latter allows for a function which could, for example, extract a known attribute to be used as the flavouring.</para>
    /// 
    /// <para>This plugin can handle attributes, child nodes and text values. Due to the current usage paradigm, CData and comment nodes
    /// are simply ignored</para></remarks>
    public class SystemXmlPlugin : IObjectFlatteningPlugin
    {
        #region Data Model

        /// <summary>
        /// Get / set the string used to separate parent objects from child objects
        /// </summary>
        /// <remarks>For consistency with xpath approaches one might use '/', but for consistency with other plugins, the default is '.'</remarks>
        public string NodeSplitString { get; set; } = ".";

        /// <summary>
        /// Get / set how the plugin should behave when multiple child nodes with the same node are found
        /// </summary>
        public DuplicateElementNameBehaviour DuplicateElementNameBehaviour { get; set; } = DuplicateElementNameBehaviour.ReportAsIs;

        /// <summary>
        /// Get / set how to behave when processing empty leaf nodes (those with no child nodes)
        /// </summary>
        public LeafNodeReportingBehaviour LeafNodeReportingBehaviour { get; set; } = LeafNodeReportingBehaviour.ReportEmptyStringIfNoAttributes;

        /// <summary>
        /// Get / set how the nodes (elements only) should be named in the output results
        /// </summary>
        /// <remarks>Note that if this is set to <see cref="NodeNamingMode.CustomFunc"/> then <see cref="NodeNamingCustomFunc"/> must be specified</remarks>
        public NodeNamingMode NodeNamingMode { get; set; } = NodeNamingMode.Standard;

        /// <summary>
        /// Get / set the custom func to use to decide the name of a given node in the output when <see cref="NodeNamingMode"/> is equal to <see cref="SystemXml.NodeNamingMode.CustomFunc"/>
        /// </summary>
        /// <remarks>The parameters are prefix, the node itself </remarks>
        public Func<string, XmlNode, IReadOnlyList<XmlNode>, string> NodeNamingCustomFunc { get; set; }

        #endregion

        public bool CanHandle(string prefix, object @object)
        {
            return @object is XmlNode;
        }

        public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
        {
            var xmlNode = (XmlNode)@object;
            return FlattenNode(objectFlattener, prefix, xmlNode);
        }

        private IEnumerable<KeyValuePair<string, object>> FlattenNode(IObjectFlattener objectFlattener, string prefix, XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                    {
                        var xmlDoc = (XmlDocument)node;

                        var adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? string.Empty : this.NodeSplitString)}{xmlDoc.DocumentElement.Name}";
                        foreach (var response in objectFlattener.FlattenObject(adjustedName, xmlDoc.DocumentElement))
                            yield return response;

                        yield break;
                    }

                case XmlNodeType.Attribute:
                    {
                        var xmlAttr = (XmlAttribute)node;
                        yield return new KeyValuePair<string, object>(prefix, xmlAttr.Value);
                        yield break;
                    }

                case XmlNodeType.Element:
                    {
                        var xmlElement = (XmlElement)node;
                        // Handle attributes...
                        foreach (XmlAttribute attr in xmlElement.Attributes)
                        {
                            var adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? string.Empty : this.NodeSplitString)}@{attr.Name}";
                            foreach (var response in objectFlattener.FlattenObject(adjustedName, attr))
                                yield return response;
                        }

                        // Extract the set of nodes which we'll actually process..
                        var childNodesToProcess = xmlElement.ChildNodes.Cast<XmlNode>().
                            Where(cn => cn.NodeType == XmlNodeType.Text || cn.NodeType == XmlNodeType.Element).
                            ToList();

                        // Handle special cases here...
                        if (this.NodeNamingMode == NodeNamingMode.Standard)
                        {
                            if (childNodesToProcess.Count == 1 && childNodesToProcess[0].NodeType == XmlNodeType.Text)
                            {
                                foreach (var response in objectFlattener.FlattenObject(prefix, childNodesToProcess[0]))
                                    yield return response;
                                yield break;
                            }
                        }

                        // Standard rules apply
                        if (childNodesToProcess.Count == 0)
                        {
                            switch (this.LeafNodeReportingBehaviour)
                            {
                                case LeafNodeReportingBehaviour.None:
                                    break;

                                case LeafNodeReportingBehaviour.ReportEmptyStringIfNoAttributesAndNodeIsNotEmpty:
                                    if (xmlElement.Attributes.Count == 0 && !xmlElement.IsEmpty)
                                        yield return new KeyValuePair<string, object>(prefix, string.Empty);
                                    break;

                                case LeafNodeReportingBehaviour.ReportEmptyStringIfNoAttributes:
                                    if (xmlElement.Attributes.Count == 0)
                                        yield return new KeyValuePair<string, object>(prefix, string.Empty);

                                    break;

                                case LeafNodeReportingBehaviour.AlwaysReportEmptyString:
                                    yield return new KeyValuePair<string, object>(prefix, string.Empty);
                                    break;
                            }

                            yield break;
                        }

                        // Time to work out if there are any duplicates..
                        // Because we'd like to retain the original order if possible (it might be useful for some consumers)
                        // then we have to do a spot of ... manipulation 
                        IDictionary<string, List<XmlNode>> childNodesByName = null;
                        if (this.NodeNamingMode == NodeNamingMode.Standard)
                        {
                            childNodesByName = new Dictionary<string, List<XmlNode>>();
                            foreach (XmlNode childNode in childNodesToProcess)
                            {
                                if (!childNodesByName.TryGetValue(childNode.Name, out var groupedChildNodes))
                                {
                                    groupedChildNodes = new List<XmlNode>(5);
                                    childNodesByName[childNode.Name] = groupedChildNodes;
                                }

                                groupedChildNodes.Add(childNode);
                            }
                        }

                        foreach (XmlNode childNode in childNodesToProcess)
                        {
                            string outputNodeName = childNode.Name;
                            switch (this.NodeNamingMode)
                            {
                                case NodeNamingMode.Standard:
                                    {
                                        var childNodeNameGroup = childNodesByName[childNode.Name];

                                        if (childNodeNameGroup.Count > 1)
                                        {
                                            // There are duplicates, so we need to respect the requested behaviour
                                            switch (this.DuplicateElementNameBehaviour)
                                            {
                                                case DuplicateElementNameBehaviour.ReportAsIs:
                                                    break;

                                                case DuplicateElementNameBehaviour.AppendIndexWithinDuplicateCollectionZeroBased:
                                                    {
                                                        int idx = childNodeNameGroup.IndexOf(childNode);
                                                        outputNodeName = $"{outputNodeName}[{idx}]";
                                                    }
                                                    break;

                                                case DuplicateElementNameBehaviour.AppendIndexWithinDuplicateCollectionOneBased:
                                                    {
                                                        int idx = childNodeNameGroup.IndexOf(childNode);
                                                        outputNodeName = $"{outputNodeName}[{idx + 1}]";
                                                    }
                                                    break;

                                                case DuplicateElementNameBehaviour.Skip:
                                                    continue;

                                                case DuplicateElementNameBehaviour.Throw:
                                                    throw new InvalidOperationException($"Found duplicate child node with name '{childNode.Name}'");

                                                default:
                                                    throw new InvalidOperationException($"Invalid duplicate element name behaviour mode specified - '{this.DuplicateElementNameBehaviour}'");
                                            }
                                        }
                                    }
                                    break;

                                case NodeNamingMode.CustomFunc:
                                    {
                                        if (this.NodeNamingCustomFunc == null)
                                            throw new InvalidOperationException("No custom func provided for NodeNamingMode=CustomFunc");

                                        outputNodeName = this.NodeNamingCustomFunc(prefix, childNode, childNodesToProcess);
                                    }
                                    break;

                                default:
                                    throw new InvalidOperationException($"Invalid node naming mode '{this.NodeNamingMode}'");
                            }

                            var adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? string.Empty : this.NodeSplitString)}{outputNodeName}";
                            foreach (var response in objectFlattener.FlattenObject(adjustedName, childNode))
                                yield return response;
                        }

                        yield break;
                    }

                case XmlNodeType.Text:
                    {
                        yield return new KeyValuePair<string, object>(prefix, node.Value);
                        yield break;
                    }
            }
        }
    }
}
