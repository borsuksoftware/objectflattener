using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace BorsukSoftware.ObjectFlattener
{
	public class ObjectFlattenerTests
	{
		[Fact]
		public void MissingPlugin_Throw()
		{
			var flattener = new ObjectFlattener
			{
				NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Throw
			};

			Assert.Throws<InvalidOperationException>(() => flattener.FlattenObject(null, new DateTime(2020, 03, 12)).ToList());
		}

		[Fact]
		public void MissingPlugin_ReturnValue()
		{
			var flattener = new ObjectFlattener
			{
				NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.ReturnValue
			};

			var @object = new
			{
				val = new DateTime(2020, 03, 12)
			};

			var output = flattener.FlattenObject("key", @object);
			Assert.NotNull(output);
			var outputAsList = output.ToList();
			Assert.Single(outputAsList);

			Assert.Equal("key", outputAsList[0].Key);
			Assert.Same(@object, outputAsList[0].Value);
		}

		[Fact]
		public void MissingPlugin_Ignore()
		{
			var flattener = new ObjectFlattener
			{
				NoAvailablePluginBehaviour = NoAvailablePluginBehaviour.Ignore
			};

			var @object = new
			{
				val = new DateTime(2020, 03, 12)
			};

			var output = flattener.FlattenObject("key", @object);
			Assert.NotNull(output);
			var outputAsList = output.ToList();
			Assert.Empty(outputAsList);
		}
	}
}
