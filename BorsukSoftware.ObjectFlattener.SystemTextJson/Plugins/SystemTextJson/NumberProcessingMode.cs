using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemTextJson
{
	/// <summary>
	/// Enum detaiing how a number element should be interpretted by the flattener
	/// </summary>
	/// <remarks>These values map directly to the underlying methods on <see cref="System.Text.Json.JsonElement"/></remarks>
	public enum NumberProcessingMode
	{
		/// <summary>
		/// Use a custom function to decide on a per value basis
		/// </summary>
		Custom,

		Float,
		Double,
		Decimal,
		SByte,
		Int16,
		Int32,
		Int64,
		Byte,
		UInt16,
		UInt32,
		UInt64,
	}
}
