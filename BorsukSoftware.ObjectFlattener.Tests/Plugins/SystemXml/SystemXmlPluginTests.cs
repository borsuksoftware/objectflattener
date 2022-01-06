using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemXml
{
    public class SystemXmlPluginTests
    {
        #region Standard naming

        [Fact]
        public void Standard_LeafNodeModeNone()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.None,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXml.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.@rootAttr", "3"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode11.childNode112.@attr", "bob"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode14", "Free format text")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_LeafNodeModeReportEmptyStringIfNoAttributesAndNodeIsNotEmpty()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.ReportEmptyStringIfNoAttributesAndNodeIsNotEmpty,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXml.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.@rootAttr", "3"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode11.childNode112.@attr", "bob"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode12", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode14", "Free format text")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_LeafNodeModeReportEmptyStringIfNoAttributes()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.ReportEmptyStringIfNoAttributes,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXml.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.@rootAttr", "3"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode11.childNode112.@attr", "bob"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode12", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode13", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode14", "Free format text")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Standard_LeafNodeModeAlwaysReportEmptyString()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.AlwaysReportEmptyString,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXml.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.@rootAttr", "3"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode11.childNode112.@attr", "bob"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode11.childNode112", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode12", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode13", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode14", "Free format text")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        #endregion

        #region Duplicate Name Handling

        [Fact]
        public void DuplicateNames_ReportAsIs()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.None,
                DuplicateElementNameBehaviour = DuplicateElementNameBehaviour.ReportAsIs,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXmlDuplicates.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode.@name", "bob1"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode.@name", "bob2"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode2.@name", "bab"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode.@name", "bob3")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void DuplicateNames_AppendIndexWithinDuplicateCollectionZeroBased()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.None,
                DuplicateElementNameBehaviour = DuplicateElementNameBehaviour.AppendIndexWithinDuplicateCollectionZeroBased,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXmlDuplicates.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode[0].@name", "bob1"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode[1].@name", "bob2"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode2.@name", "bab"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode[2].@name", "bob3")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }
        [Fact]
        public void DuplicateNames_AppendIndexWithinDuplicateCollectionOneBased()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.None,
                DuplicateElementNameBehaviour = DuplicateElementNameBehaviour.AppendIndexWithinDuplicateCollectionOneBased,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXmlDuplicates.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode[1].@name", "bob1"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode[2].@name", "bob2"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode2.@name", "bab"),
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode[3].@name", "bob3")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void DuplicateNames_Skip()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.None,
                DuplicateElementNameBehaviour = DuplicateElementNameBehaviour.Skip,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXmlDuplicates.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.childNode1.childNode2.@name", "bab")
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void DuplicateNames_Throw()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.None,
                DuplicateElementNameBehaviour = DuplicateElementNameBehaviour.Throw,
                NodeNamingMode = NodeNamingMode.Standard
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXmlDuplicates.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            Assert.Throws<InvalidOperationException>(() => plugin.FlattenObject(flattener, null, xmlDoc).ToList());
        }

        #endregion

        #region Custom Naming

        [Fact]
        public void CustomNaming()
        {
            var plugin = new SystemXmlPlugin
            {
                LeafNodeReportingBehaviour = LeafNodeReportingBehaviour.AlwaysReportEmptyString,
                NodeNamingMode = NodeNamingMode.CustomFunc,
                NodeNamingCustomFunc = (string prefix, System.Xml.XmlNode node, IReadOnlyList<System.Xml.XmlNode> nodes) =>
                {
                    var name = node.Attributes["name"]?.Value;
                    if (!string.IsNullOrEmpty(name))
                    {
                        return $"{node.Name}-{name}";
                    }
                    else
                        return node.Name;
                }
            };

            var flattener = new ObjectFlattener();
            flattener.Plugins.Add(plugin);

            var xmlDoc = LoadXmlDocFromResourceNameFragment("Resources.SystemXmlPluginTests.sampleXmlCustomNames.xml");

            Assert.True(plugin.CanHandle(null, xmlDoc.DocumentElement));

            var allFlattenedValues = plugin.FlattenObject(flattener, null, xmlDoc).ToList();

            var expectedValues = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>( "myDoc.childNode-bob1.@name", "bob1"),
                new KeyValuePair<string, object>( "myDoc.childNode-bob1.childNode11", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode-bob2.@name", "bob2"),
                new KeyValuePair<string, object>( "myDoc.childNode-bob2.childNode11", string.Empty),
                new KeyValuePair<string, object>( "myDoc.childNode-bob3.@name", "bob3"),
                new KeyValuePair<string, object>( "myDoc.childNode-bob3.childNode11", string.Empty),
            };

            allFlattenedValues.Should().BeEquivalentTo(expectedValues);
        }

        #endregion

        #region Utils

        private static System.Xml.XmlDocument LoadXmlDocFromResourceNameFragment( string endFragment)
        {
            var resourceName = typeof(SystemXmlPluginTests).Assembly.GetManifestResourceNames().Where(n => n.EndsWith(endFragment)).Single();
            var xmlDoc = new System.Xml.XmlDocument();
            using (var stream = typeof(SystemXmlPluginTests).Assembly.GetManifestResourceStream(resourceName))
                xmlDoc.Load(stream);

            return xmlDoc;
        }

        #endregion
    }
}
