using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class StandardPluginTests
	{
		public static IEnumerable<object[]> LeafDataTypes
		{
			get
			{
				var dict = new Dictionary<string, object>();

				dict["datetime"] = new DateTime(2020, 03, 12);
				dict["datetimeoffset"] = new DateTimeOffset(new DateTime(2020, 03, 12), new TimeSpan(1, 0, 0));
				dict["timespan"] = new TimeSpan(1, 10, 15);
				dict["guid"] = Guid.NewGuid();

				dict["intptr"] = new IntPtr(230);
				dict["uintptr"] = new UIntPtr(231);

				dict["bigint"] = new System.Numerics.BigInteger(23);
				dict["complex"] = new System.Numerics.Complex(232.3, -224);

				dict["str"] = "bob";
				dict["bool"] = true;

				dict["sbyte"] = (sbyte)-2;
				dict["short"] = (short)-27000;
				dict["int"] = (int)-128000;
				dict["long"] = (long)-8000000000L;

				dict["byte"] = (byte)2;
				dict["ushort"] = (ushort)27000;
				dict["uint"] = (uint)128000;
				dict["ulong"] = (ulong)8000000000L;

				dict["float"] = 2.3F;
				dict["double"] = 2343.34;
				dict["decimal"] = new decimal(2342.23);

				foreach (var pair in dict)
					yield return new object[] { pair.Key, pair.Value };
			}
		}

		public static IEnumerable<object[]> LeafDataTypesStructs => LeafDataTypes.Where(array => !array[1].GetType().IsClass);

		[MemberData(nameof(LeafDataTypes))]
		[Theory]
		public void LeafTest(string name, object val)
		{
			var plugin = new StandardPlugin();

			Assert.True(plugin.CanHandle(name, val));
			var output = plugin.FlattenObject(null, name, val);
			Assert.NotNull(output);
			var outputAsList = output.ToList();
			Assert.Single(outputAsList);
			Assert.Equal(name, outputAsList[0].Key);
			Assert.Equal(val, outputAsList[0].Value);
		}

		[MemberData(nameof(LeafDataTypesStructs))]
		[Theory]
		public void NullableLeafTest(string name, object val)
		{
			var plugin = new StandardPlugin();

			var valType = val.GetType();
			var nullableType = typeof(Nullable<>).MakeGenericType(valType);
			var nullableValue = Activator.CreateInstance(nullableType, new object[] { val });

			Assert.True(plugin.CanHandle(name, val));
			var output = plugin.FlattenObject(null, name, nullableValue);
			Assert.NotNull(output);
			var outputAsList = output.ToList();
			Assert.Single(outputAsList);
			Assert.Equal(name, outputAsList[0].Key);
			Assert.Equal(val, outputAsList[0].Value);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public void OnlyPublicReadablePropertiesEtcAreVisible(bool processProperties, bool processFields)
		{
			var item = new TestClass();

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new StandardPlugin()
			{
				ProcessFields = processFields,
				ProcessProperties = processProperties
			});

			var output = flattener.FlattenObject(null, item);
			Assert.NotNull(output);
			var outputAsDictionary = output.ToDictionary(p => p.Key, p => p.Value);

			if (processProperties)
			{
				Assert.True(outputAsDictionary.TryGetValue("Cat", out var catVal));
				Assert.Equal(item.Cat, catVal);
			}
			else
				Assert.False(outputAsDictionary.ContainsKey("Cat"));

			if (processFields)
			{
				Assert.True(outputAsDictionary.TryGetValue("Billy", out var billyVal));
				Assert.Equal(item.Billy, billyVal);
			}
			else
				Assert.False(outputAsDictionary.ContainsKey("Billy"));
		}

		private class TestClass
		{
			public static string StaticBob { set { } }
			public string Bob { set { } }

			public static string StaticCat => "2";
			public string Cat => "2";

			public string this[int index] => "a";

#pragma warning disable CS0649,CS0169
			public static string StaticBilly;
			public string Billy;
			private string BillyBob;
#pragma warning restore CS0649, CS0169
		}

		[Fact]
		public void ValidateEnumsAreTreatedAsLeafTypes()
		{
			var plugin = new StandardPlugin();

			var @object = new
			{
				enumValue = DateTimeKind.Local
			};

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(plugin);

			var output = flattener.FlattenObject(null, @object);
			Assert.NotNull(output);
			var outputAsList = output.ToList();
			Assert.Single(outputAsList);

			Assert.Equal("enumValue", outputAsList[0].Key);
			Assert.Equal(DateTimeKind.Local, outputAsList[0].Value);
		}
	}
}