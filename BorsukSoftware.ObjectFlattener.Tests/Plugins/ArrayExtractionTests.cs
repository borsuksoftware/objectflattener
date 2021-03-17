using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	public class ArrayExtractionTests
	{
		[Fact]
		public void Extract2DArray()
		{
			var o = new
			{
				array = new int[2, 3]
			};

			o.array[0, 0] = 4;
			o.array[0, 1] = 5;
			o.array[0, 2] = 6;
			o.array[1, 0] = 7;
			o.array[1, 1] = 8;
			o.array[1, 2] = 9;

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var output = flattener.FlattenObject(null, o);

			Assert.NotNull(output);
			var outputAsList = output.ToList();

			Assert.Equal(6, outputAsList.Count);

			Assert.Contains(new KeyValuePair<string, object>("array[0,0]", 4), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("array[0,1]", 5), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("array[0,2]", 6), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("array[1,0]", 7), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("array[1,1]", 8), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("array[1,2]", 9), outputAsList);
		}

		[Fact]
		public void JaggedArrays()
		{
			var o = new
			{
				jaggedArray = new int[4][]
			};

			o.jaggedArray[0] = new int[] { 1, 4, 7 };
			o.jaggedArray[1] = new int[0];
			o.jaggedArray[2] = null;
			o.jaggedArray[3] = new int[] { 1, 2 };

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var output = flattener.FlattenObject(null, o);

			Assert.NotNull(output);
			var outputAsList = output.ToList();

			Assert.Equal(6, outputAsList.Count);

			Assert.Contains(new KeyValuePair<string, object>("jaggedArray[0][0]", 1), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("jaggedArray[0][1]", 4), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("jaggedArray[0][2]", 7), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("jaggedArray[2]", null), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("jaggedArray[3][0]", 1), outputAsList);
			Assert.Contains(new KeyValuePair<string, object>("jaggedArray[3][1]", 2), outputAsList);
		}

		[Fact]
		public void ExtractEmptyArray()
		{
			var o = new
			{
				array = Array.Empty<int>()
			};

			var flattener = new ObjectFlattener();
			flattener.Plugins.Add(new ArrayPlugin());
			flattener.Plugins.Add(new StandardPlugin());

			var output = flattener.FlattenObject(null, o);

			Assert.NotNull(output);
			var outputAsList = output.ToList();
			Assert.Empty(outputAsList);
		}
	}
}
