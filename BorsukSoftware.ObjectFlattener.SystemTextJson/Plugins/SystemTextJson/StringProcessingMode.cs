using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemTextJson
{
	/// <summary>
	/// Enum detaiing how a string element should be interpretted by the flattener
	/// </summary>
	/// <remarks>These values map directly to the underlying methods on <see cref="System.Text.Json.JsonElement"/></remarks>
	public enum StringProcessingMode
	{
		/// <summary>
		/// Use a custom function to decide on a per value basis
		/// </summary>
		Custom,
		String,
		DateTime,
		DateTimeOffset,
		Guid,
		Base64
	}
}
