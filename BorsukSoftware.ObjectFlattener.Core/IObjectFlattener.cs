using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener
{
	/// <summary>
	/// Standard interface for object flattener
	/// </summary>
	public interface IObjectFlattener
	{
		/// <summary>
		/// Flatten the given source object to its constituent values
		/// </summary>
		/// <param name="prefix">The prefix to use (may be null)</param>
		/// <param name="sourceObject">The object to flatten</param>
		/// <returns>An enumerable over all of the flattened values</returns>
		IEnumerable<KeyValuePair<string, object>> FlattenObject(string prefix, object sourceObject);
	}
}
