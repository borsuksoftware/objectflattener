using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class ListTests
	{
		[Theory]
		[InlineData(null)]
		[InlineData("bob")]
		public void Standard(string prefix)
		{
			var list = new System.Collections.ArrayList(10);
			list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

			var flattenerMock = new Mock<IObjectFlattener>();
			flattenerMock.Setup(x => x.FlattenObject(It.IsAny<string>(), It.IsAny<int>())).
				 Returns((string p, object o) => new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(p, $"-{o}") });

			var plugin = new ListPlugin();
			var output = plugin.FlattenObject(flattenerMock.Object, prefix, list);
			Assert.NotNull(output);
			var outputAsList = output.ToDictionary(p => p.Key, p => p.Value);

			for (int i = 0; i < list.Count; ++i)
			{
				Assert.True(outputAsList.TryGetValue($"{prefix}[{i}]", out var val));
				Assert.Equal($"-{i + 1}", val);
			}
		}
	}
}