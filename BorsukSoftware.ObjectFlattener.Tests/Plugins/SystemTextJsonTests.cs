using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class SystemTextJsonTests
	{
		[Fact]
		public void General()
		{
			var o = new
			{
				g = 2.3,
				h = "st",
				i = new object[] { new { sd = 2.9, dfg = 1.8 }, new { df = 2.3 } },
				j = new
				{
					g = new
					{
						g = new double[] { 56.3, 23.1, 245.1, 4.2, 22.7, 2.12 }
					}
				}
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(o);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin());
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var beforeSerialization = flattener.FlattenObject(null, o);
			var afterSerialization = flattener.FlattenObject(null, deserialized);

			TestUtils.ValidateOutputsAreEqual(beforeSerialization, afterSerialization);
		}

		public static IEnumerable<object[]> NumberProcessingModesData
		{
			get
			{
				yield return new object[] { SystemTextJson.NumberProcessingMode.Decimal, 2.03m };

				yield return new object[] { SystemTextJson.NumberProcessingMode.SByte, (sbyte)-4 };
				yield return new object[] { SystemTextJson.NumberProcessingMode.Int16, (short)-234 };
				yield return new object[] { SystemTextJson.NumberProcessingMode.Int32, -2345235 };
				yield return new object[] { SystemTextJson.NumberProcessingMode.Int64, -50000000000 };

				yield return new object[] { SystemTextJson.NumberProcessingMode.Byte, (byte)4 };
				yield return new object[] { SystemTextJson.NumberProcessingMode.UInt16, (ushort)234 };
				yield return new object[] { SystemTextJson.NumberProcessingMode.UInt32, 2345235U };
				yield return new object[] { SystemTextJson.NumberProcessingMode.UInt64, 50000000000UL };
			}
		}

		[MemberData(nameof(NumberProcessingModesData))]
		[InlineData(SystemTextJson.NumberProcessingMode.Float, 2.0F)]
		[InlineData(SystemTextJson.NumberProcessingMode.Double, 2.0)]
		[Theory]
		public void NumberProcessingModes(SystemTextJson.NumberProcessingMode numberProcessingMode, object value)
		{
			var @object = new
			{
				property = value
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(@object);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin() { NumberProcessingMode = numberProcessingMode });
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var beforeSerialization = flattener.FlattenObject(null, @object);
			var afterSerialization = flattener.FlattenObject(null, deserialized);

			TestUtils.ValidateOutputsAreEqual(beforeSerialization, afterSerialization);
		}

		[Fact]
		public void CustomNumberProcessing()
		{
			var @object = new
			{
				property = 2.3m
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(@object);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin()
			{
				NumberProcessingMode = SystemTextJson.NumberProcessingMode.Custom,
				NumberExtractionFunc = (p, e) => { return e.GetDecimal(); }
			});
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var beforeSerialization = flattener.FlattenObject(null, @object);
			var afterSerialization = flattener.FlattenObject(null, deserialized);

			TestUtils.ValidateOutputsAreEqual(beforeSerialization, afterSerialization);
		}

		[Fact]
		public void CustomNumberProcessingWithoutFuncFails()
		{
			var @object = new
			{
				property = 2.3m
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(@object);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin()
			{
				NumberProcessingMode = SystemTextJson.NumberProcessingMode.Custom,
			});
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			Assert.ThrowsAny<Exception>(() => flattener.FlattenObject(null, deserialized).ToList());
		}

		public static IEnumerable<object[]> StringProcessingModesData
		{
			get
			{
				yield return new object[] { SystemTextJson.StringProcessingMode.DateTime, DateTime.Today };
				yield return new object[] { SystemTextJson.StringProcessingMode.DateTimeOffset, new DateTimeOffset(new DateTime(2020, 03, 16), new TimeSpan(1, 0, 0)) };

				yield return new object[] { SystemTextJson.StringProcessingMode.Guid, Guid.NewGuid() };
				yield return new object[] { SystemTextJson.StringProcessingMode.String, "bob" };

				yield return new object[] { SystemTextJson.StringProcessingMode.Base64, new byte[] { 1, 34, 45, 2 } };
			}
		}

		[MemberData(nameof(StringProcessingModesData))]
		[Theory]
		public void StringProcessingModes(SystemTextJson.StringProcessingMode stringProcessingMode, object value)
		{
			var @object = new
			{
				property = value
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(@object);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin() { StringProcessingMode = stringProcessingMode });
			flattener.Plugins.Add(new Plugins.FunctionBasedPlugin((p, o) => o is byte[], (of, p, o) => new[] { new KeyValuePair<string, object>(p, o) }));
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var beforeSerialization = flattener.FlattenObject(null, @object);
			var afterSerialization = flattener.FlattenObject(null, deserialized);

			TestUtils.ValidateOutputsAreEqual(beforeSerialization, afterSerialization);
		}

		[Fact]
		public void CustomStringProcessing()
		{
			var @object = new
			{
				property = DateTime.Today
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(@object);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin()
			{
				StringProcessingMode = SystemTextJson.StringProcessingMode.Custom,
				StringExtractionFunc = (p, e) => { return e.GetDateTime(); }
			});
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var beforeSerialization = flattener.FlattenObject(null, @object);
			var afterSerialization = flattener.FlattenObject(null, deserialized);

			TestUtils.ValidateOutputsAreEqual(beforeSerialization, afterSerialization);
		}

		[Fact]
		public void CustomStringProcessingWithoutFuncFails()
		{
			var @object = new
			{
				property = DateTime.Today
			};

			var deserialized = TestUtils.RoundTripViaSystemTextJson(@object);

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new SystemTextJson.SystemTextJsonPlugin()
			{
				StringProcessingMode = SystemTextJson.StringProcessingMode.Custom
			});
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			Assert.ThrowsAny<Exception>(() => flattener.FlattenObject(null, deserialized).ToList());
		}

	}
}
