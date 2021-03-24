using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
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

		[Fact]
		public void GenericList()
		{
			var list = new List<string>(new[] { "1", "bob", "bill" });

			var plugin = new ListPlugin()
			{
				HandleGenericLists = true,
				HandleNonGenericLists = false,
				HandleReadOnlyLists = false
			};
			Assert.True(plugin.CanHandle(null, list));

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new StandardPlugin());

			var output = plugin.FlattenObject(flattener, null, list);
			Assert.NotNull(output);

			var outputAsList = output.ToList();

			var expectedOutput = new[]
			{
				new KeyValuePair<string,object>( "[0]", "1"),
				new KeyValuePair<string,object>( "[1]", "bob"),
				new KeyValuePair<string,object>( "[2]", "bill")
			};

			outputAsList.Should().BeEquivalentTo(expectedOutput);
		}

		[Fact]
		public void FSharpLists()
		{
			var list = new List<string>(new[] { "1", "bob", "bill" });

			var fsharpList = new Microsoft.FSharp.Collections.FSharpList<string>("bill", Microsoft.FSharp.Collections.FSharpList<string>.Empty);
			fsharpList = new Microsoft.FSharp.Collections.FSharpList<string>("bob", fsharpList);
			fsharpList = new Microsoft.FSharp.Collections.FSharpList<string>("1", fsharpList);

			var fsharpListAsManuallyFlattened = fsharpList.Select((item, idx) => new KeyValuePair<string, object>($"[idx]", item));
			
			// Check that we've flattened correctly..
			list.Select((item, idx) => new KeyValuePair<string, object>($"[idx]", item)).
			 	Should().
			 	BeEquivalentTo(fsharpListAsManuallyFlattened);

			var plugin = new ListPlugin();
			Assert.True(plugin.CanHandle(null, fsharpList));

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new StandardPlugin());

			var output = plugin.FlattenObject(flattener, null, fsharpList);
			Assert.NotNull(output);

			var outputAsList = output.ToList();

			var expectedOutput = new[]
			{
				new KeyValuePair<string,object>( "[0]", "1"),
				new KeyValuePair<string,object>( "[1]", "bob"),
				new KeyValuePair<string,object>( "[2]", "bill")
			};

			outputAsList.Should().BeEquivalentTo(expectedOutput);
		}
	}
}