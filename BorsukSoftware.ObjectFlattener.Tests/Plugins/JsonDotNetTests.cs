using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class JsonDotNetTests
	{
		[Fact]
		public void Standard()
		{
			var o = new
			{
				g = 2L,
				h = "st",
				i = new object[] { new { sd = 2L, dfg = 1L }, new { df = 2.3 } },
				j = new
				{
					g = new
					{
						g = new long[] { 56L, 23L, 245L, 4L, 22L, 2L }
					}
				}
			};

			System.IO.StringWriter sw = new System.IO.StringWriter();
			var serializer = new Newtonsoft.Json.JsonSerializer();
			using (var textWriter = new Newtonsoft.Json.JsonTextWriter(sw))
			{
				serializer.Serialize(textWriter, o);
			}

			object deserialized;
			using (var reader = new System.IO.StringReader(sw.ToString()))
			{
				using var reader2 = new Newtonsoft.Json.JsonTextReader(reader);
				deserialized = serializer.Deserialize<object>(reader2);
			}

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new JsonDotNetPlugin());
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var beforeSerialization = flattener.FlattenObject(null, o);
			var afterSerialization = flattener.FlattenObject(null, deserialized);

			TestUtils.ValidateOutputsAreEqual(beforeSerialization, afterSerialization);
		}
	}
}
