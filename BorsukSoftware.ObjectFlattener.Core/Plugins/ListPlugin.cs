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
		#region Data model

		/// <summary>
		/// Get / set whether or not to handle instances of <see cref="IList"/>
		/// </summary>
		public bool HandleNonGenericLists { get; set; } = true;

		/// <summary>
		/// Get / set whether or not to handle instances of <see cref="IList{T}"/>
		/// </summary>
		public bool HandleGenericLists { get; set; } = true;

		/// <summary>
		/// Get / set whether or not to handle instances of <see cref="System.Collections.Generic.IReadOnlyList{T}"/>
		/// </summary>
		public bool HandleReadOnlyLists { get; set; } = true;

		#endregion

		public bool CanHandle(string prefix, object @object)
		{
			if (@object == null)
				return false;

			if (this.HandleNonGenericLists && @object is IList)
				return true;

			if (this.HandleGenericLists)
			{
				var objectType = @object.GetType();
				if (objectType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
					return true;
			}

			if (this.HandleReadOnlyLists)
			{
				var objectType = @object.GetType();
				if (objectType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
					return true;
			}

			return false;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			var objectAsList = (IEnumerable)@object;
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
