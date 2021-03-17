using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Standard plugin to take apart regular arrays and cleave them into their underlying components
	/// </summary>
	/// <remarks>This plugin has the limitation in that for an empty array, nothing is returned.</remarks>
	public class ArrayPlugin : IObjectFlatteningPlugin
	{
		public bool CanHandle(string prefix, object @object)
		{
			return @object is Array;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			var objectAsArray = (Array)@object;

			var ranks = new List<Tuple<int, int>>(objectAsArray.Rank);
			for (int rank = 0; rank < objectAsArray.Rank; ++rank)
			{
				var upperBound = objectAsArray.GetUpperBound(rank);
				var lowerBound = objectAsArray.GetLowerBound(rank);

				if (upperBound == -1)
					yield break;

				ranks.Add(Tuple.Create<int, int>(lowerBound, upperBound));
			}

			int[] indices = new int[ranks.Count];
			for (int i = 0; i < indices.Length; ++i)
				indices[i] = ranks[i].Item1;

			// Handle zero length-arrays??
			while (true)
			{
				var value = objectAsArray.GetValue(indices);

				var joinedIndices = string.Join(",", indices);
				var adjustedName = $"{prefix}[{joinedIndices}]";

				foreach (var entry in objectFlattener.FlattenObject(adjustedName, value))
					yield return entry;

				bool overflow = false;
				int indexToUpdate = indices.Length - 1;
				while (!overflow)
				{
					indices[indexToUpdate]++;
					if (indices[indexToUpdate] <= ranks[indexToUpdate].Item2)
						break;

					indices[indexToUpdate] = ranks[indexToUpdate].Item1;
					indexToUpdate--;
					if (indexToUpdate < 0)
						overflow = true;
				}

				if (overflow)
					break;
			}
		}
	}
}
