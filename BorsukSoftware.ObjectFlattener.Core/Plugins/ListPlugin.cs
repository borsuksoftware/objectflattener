using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Standard plugin to take apart regular arrays and cleave them into their underlying components
	/// </summary>
	/// <remarks>This plugin has the capacity to handle arrays, however <see cref="ArrayPlugin"/> should be used to support multi-dimensional arrays.</remarks>
	public class ListPlugin : IObjectFlatteningPlugin
	{
		public bool CanHandle(string prefix, object @object)
		{
			return @object is IList;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			var objectAsList = (IList)@object;
			var output = objectAsList.Cast<object>().
				Select((o, idx) => new
				{
					o,
					adjustedName = $"{prefix}[{idx}]"
				}).
				SelectMany(tuple => objectFlattener.FlattenObject(tuple.adjustedName, tuple.o));

			return output;
		}
	}
}
