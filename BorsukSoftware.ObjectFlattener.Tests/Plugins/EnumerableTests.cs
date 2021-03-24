using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class EnumerableTests
	{
		[Fact]
		public void Standard()
		{
			var plugin = new EnumerablePlugin();

			var enumerable = Enumerable.Range(0, 10).Select(i => $"{5 + i}");

			Assert.True(plugin.CanHandle(null, enumerable));

			var objectFlattener = new ObjectFlattener();
			objectFlattener.Plugins.Add(new StandardPlugin());
			var output = plugin.FlattenObject(objectFlattener, null, enumerable);

			Assert.NotNull(output);

			var outputAsList = output.ToList();

			var expectedOutput = enumerable.Select((item, idx) => new KeyValuePair<string, object>($"[{idx}]", item));

			outputAsList.Should().BeEquivalentTo(expectedOutput);
		}
	}
}
