using System;
using System.Collections.Generic;
using System.Linq;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Plugin to handle implementations of <see cref="System.Collections.IEnumerable"/>
	/// </summary>
	/// <remarks>This plugin isn't really recommended for many general purpose use-cases for multiple reasons.
	/// 
	/// 1. It can capture a very large number of items, e.g. strings and the like, which really aren't intended. 
	/// 2. If this operates upon unordered collections, then the address returned will be based off the undefined order
	/// 
	/// It exists for completion and to handle some specific use-cases.
	/// 
	/// If you're going to use it, then consider using it in conjunction with <see cref="FilterPlugin"/> to reduce its
	/// ability to capture types
	/// </remarks>
	public class EnumerablePlugin : IObjectFlatteningPlugin
	{
		public bool CanHandle(string prefix, object @object)
		{
			return @object is System.Collections.IEnumerable;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			var enumerable = (System.Collections.IEnumerable) @object;

			return enumerable.Cast<object>().
				Select((item, idx) => objectFlattener.FlattenObject($"{prefix}[{idx}]", item)).
				SelectMany(seq => seq);
		}
	}
}
