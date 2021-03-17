using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class DictionaryPluginTests
	{
		[Fact]
		public void CheckKeyFuncIsCalled()
		{
			var @object = new
			{
				dictionary = new Dictionary<string, object>(),
				otherProperty = new
				{
					nested1 = 3
				}
			};

			@object.dictionary["key1"] = new { nestedObj = "he" };
			@object.dictionary["key2"] = 3;

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new DictionaryPlugin<string>(key => key));
			flattener.Plugins.Add(new StandardPlugin());

			var output = flattener.FlattenObject(null, @object);
			Assert.NotNull(output);
			var outputAsDictionary = output.ToDictionary(p => p.Key, p => p.Value);
			Assert.NotEmpty(outputAsDictionary);

			Assert.Equal(3, outputAsDictionary.Count);
			Assert.True(outputAsDictionary.ContainsKey("otherProperty.nested1"));
			Assert.True(outputAsDictionary.ContainsKey("dictionary.key1.nestedObj"));
			Assert.True(outputAsDictionary.ContainsKey("dictionary.key2"));
		}

		[Fact]
		public void CheckFilterTypeIsntIgnored()
		{
			var @object = new
			{
				dictionary1 = new Dictionary<string, object>(),
				dictionary2 = new Dictionary<int, object>()
			};

			@object.dictionary1["key1"] = 2;
			@object.dictionary2[2] = 3;

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new DictionaryPlugin<string>(key => key));
			flattener.Plugins.Add(new FunctionBasedPlugin((prefix, obj) => obj is IDictionary<int, object>, (objFlattener, str, obj) => Array.Empty<KeyValuePair<string, object>>()));
			flattener.Plugins.Add(new StandardPlugin());

			var output = flattener.FlattenObject(null, @object);
			Assert.NotNull(output);
			var outputAsDictionary = output.ToDictionary(p => p.Key, p => p.Value);

			Assert.Single(outputAsDictionary);
			Assert.True(outputAsDictionary.ContainsKey("dictionary1.key1"));
		}
	}
}
