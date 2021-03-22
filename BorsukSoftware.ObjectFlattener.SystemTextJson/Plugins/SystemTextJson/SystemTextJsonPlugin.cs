using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BorsukSoftware.ObjectFlattener.Plugins.SystemTextJson
{
	/// <summary>
	/// Plugin to flatten instances of <see cref="JsonElement"/> to their constituent properties
	/// </summary>
	/// <remarks>The underlying <see cref="JsonElement"/> instances don't contain enough information to detail precisely what
	/// type of value they are conceptually so it's necessary to provide this information to the plugin</remarks>
	public class SystemTextJsonPlugin : IObjectFlatteningPlugin
	{
		public NumberProcessingMode NumberProcessingMode { get; set; } = NumberProcessingMode.Double;
		public StringProcessingMode StringProcessingMode { get; set; } = StringProcessingMode.String;
		public Func<string, JsonElement, object> NumberExtractionFunc { get; set; }
		public Func<string, JsonElement, object> StringExtractionFunc { get; set; }

		public bool CanHandle(string prefix, object @object)
		{
			return @object is JsonElement;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			var element = (JsonElement)@object;
			return FlattenElement(prefix, element);
		}

		private IEnumerable<KeyValuePair<string, object>> FlattenElement(string prefix, JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					{
						var enumerator = element.EnumerateObject();
						while (enumerator.MoveNext())
						{
							var adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ".")}{enumerator.Current.Name}";
							var output = FlattenElement(adjustedName, enumerator.Current.Value);
							foreach (var entry in output)
								yield return entry;
						}

						yield break;
					}

				case JsonValueKind.Array:
					{
						var enumerator = element.EnumerateArray();
						int idx = 0;
						while (enumerator.MoveNext())
						{
							var adjustedName = $"{prefix}[{idx}]";
							var output = FlattenElement(adjustedName, enumerator.Current);
							foreach (var entry in output)
								yield return entry;

							++idx;
						}

						yield break;
					}

				case JsonValueKind.Null:
					yield return new KeyValuePair<string, object>(prefix, null);
					yield break;

				case JsonValueKind.False:
					yield return new KeyValuePair<string, object>(prefix, false);
					yield break;

				case JsonValueKind.True:
					yield return new KeyValuePair<string, object>(prefix, true);
					yield break;

				case JsonValueKind.Number:
					switch (this.NumberProcessingMode)
					{
						case NumberProcessingMode.Custom:
							{
								if (this.NumberExtractionFunc == null)
									throw new InvalidOperationException($"Custom mode specified for number extraction, but no function has been supplied ('{prefix}')");

								var value = this.NumberExtractionFunc(prefix,element);
								yield return new KeyValuePair<string, object>(prefix, value);
								yield break;
							}

						case NumberProcessingMode.Float:
							{
								if (!element.TryGetSingle(out var @float))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as a float");
								yield return new KeyValuePair<string, object>(prefix, @float);
								yield break;
							}

						case NumberProcessingMode.Double:
							{
								if (!element.TryGetDouble(out var @double))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as a double");
								yield return new KeyValuePair<string, object>(prefix, @double);
								yield break;
							}

						case NumberProcessingMode.Decimal:
							{
								if (!element.TryGetDecimal(out var @decimal))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as a decimal");
								yield return new KeyValuePair<string, object>(prefix, @decimal);
								yield break;
							}

						case NumberProcessingMode.SByte:
							{
								if (!element.TryGetSByte(out var @sbyte))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as a signed byte");
								yield return new KeyValuePair<string, object>(prefix, @sbyte);
								yield break;
							}

						case NumberProcessingMode.Int16:
							{
								if (!element.TryGetInt16(out var int16))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as an int16");
								yield return new KeyValuePair<string, object>(prefix, int16);
								yield break;
							}

						case NumberProcessingMode.Int32:
							{
								if (!element.TryGetInt32(out var int32))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as an int32");
								yield return new KeyValuePair<string, object>(prefix, int32);
								yield break;
							}

						case NumberProcessingMode.Int64:
							{
								if (!element.TryGetInt64(out var int64))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as an int64");
								yield return new KeyValuePair<string, object>(prefix, int64);
								yield break;
							}

						case NumberProcessingMode.Byte:
							{
								if (!element.TryGetByte(out var @byte))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as an unsigned byte");
								yield return new KeyValuePair<string, object>(prefix, @byte);
								yield break;
							}

						case NumberProcessingMode.UInt16:
							{
								if (!element.TryGetUInt16(out var uint16))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as a uint16");
								yield return new KeyValuePair<string, object>(prefix, uint16);
								yield break;
							}

						case NumberProcessingMode.UInt32:
							{
								if (!element.TryGetUInt32(out var uint32))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as an uint32");
								yield return new KeyValuePair<string, object>(prefix, uint32);
								yield break;
							}

						case NumberProcessingMode.UInt64:
							{
								if (!element.TryGetUInt64(out var uint64))
									throw new InvalidOperationException($"Unable to extract '{prefix}' as an uint64");
								yield return new KeyValuePair<string, object>(prefix, uint64);
								yield break;
							}

						default:
							throw new NotSupportedException($"Unknown number processing mode '{this.NumberProcessingMode}'");
					}

				case JsonValueKind.String:
					{
						switch( this.StringProcessingMode)
						{
							case StringProcessingMode.Custom:
								{
									if (this.StringExtractionFunc == null)
										throw new InvalidOperationException($"Custom mode specified for string extraction, but no function has been supplied ('{prefix}')");

									var value = this.StringExtractionFunc(prefix, element);
									yield return new KeyValuePair<string, object>(prefix, value);
									yield break;
								}
							case StringProcessingMode.Base64:
								{
									if (!element.TryGetBytesFromBase64(out var bytes))
										throw new InvalidOperationException($"Unable to process '{prefix}' as a base 64 string");
									yield return new KeyValuePair<string, object>(prefix, bytes);
									yield break;
								}

							case StringProcessingMode.String:
								{
									yield return new KeyValuePair<string, object>(prefix, element.GetString());
									yield break;
								}

							case StringProcessingMode.DateTime:
								{
									if (!element.TryGetDateTime(out var datetime))
										throw new InvalidOperationException($"Unable to process '{prefix}' as a date time string");
									yield return new KeyValuePair<string, object>(prefix, datetime);
									yield break;
								}

							case StringProcessingMode.DateTimeOffset:
								{
									if (!element.TryGetDateTimeOffset(out var datetimeoffset))
										throw new InvalidOperationException($"Unable to process '{prefix}' as a date time offset string");
									yield return new KeyValuePair<string, object>(prefix, datetimeoffset);
									yield break;
								}

							case StringProcessingMode.Guid:
								{
									if (!element.TryGetGuid(out var guid))
										throw new InvalidOperationException($"Unable to process '{prefix}' as a guid");
									yield return new KeyValuePair<string, object>(prefix, guid);
									yield break;
								}

							default:
								throw new NotSupportedException($"Unknown string processing mode '{this.StringProcessingMode}'");
						}
					}

				default:
					throw new NotImplementedException($"Unhandled ValueKind - '{element.ValueKind}'");
			}
		}
	}
}
