using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener
{
	/// <summary>
	/// Enum detailing how <see cref="ObjectFlattener"/> instances should behave if a plugin cannot 
	/// be found to handle an object to be flattened
	/// </summary>
	public enum NoAvailablePluginBehaviour
	{
		/// <summary>
		/// If no plugin is available to process an object, then throw an exception
		/// </summary>
		Throw,

		/// <summary>
		/// If no plugin is available to process an object, then simply ignore the object
		/// </summary>
		Ignore,

		/// <summary>
		/// If no plugin is available to process an object, then simply return the object
		/// </summary>
		ReturnValue
	}
}
