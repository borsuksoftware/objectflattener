using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public static class TestUtils
	{
		public static System.Text.Json.JsonElement RoundTripViaSystemTextJson( object @object )
		{
			var memoryStream = new System.IO.MemoryStream();
			var buffer = memoryStream.GetBuffer();
			using (var writer = new System.Text.Json.Utf8JsonWriter(memoryStream))
			{
				System.Text.Json.JsonSerializer.Serialize<object>(writer, @object);
				writer.Flush();
			}

			memoryStream.Position = 0;
			System.Text.Json.JsonElement deserialized;
			using (var doc = System.Text.Json.JsonDocument.Parse(memoryStream))
				deserialized = doc.RootElement.Clone();

			return deserialized;
		}

		public static void ValidateOutputsAreEqual(IEnumerable<KeyValuePair<string,object>> expected, IEnumerable<KeyValuePair<string,object>> actual)
		{
			Assert.NotNull(expected);
			Assert.NotNull(actual);

			var expectedList = expected.OrderBy(p => p.Key).ToList();
			var actualList = actual.OrderBy(p => p.Key).ToList();

			Assert.Equal(expectedList.Count, actualList.Count);

			for (int i = 0; i < expectedList.Count; ++i)
			{
				Assert.Equal(expectedList[i].Key, actualList[i].Key);
				Assert.Equal(expectedList[i].Value, actualList[i].Value);
			}
		}
	}
}
