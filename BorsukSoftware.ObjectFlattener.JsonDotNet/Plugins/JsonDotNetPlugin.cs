using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Standard plugin for dealing with instances of documents deserialized using Json.net (<see cref="JToken"/>)
	/// </summary>
	/// <remarks>This version doesn't currently have any configuration </remarks>
	public class JsonDotNetPlugin : IObjectFlatteningPlugin
	{
		public bool CanHandle(string prefix, object @object)
		{
			return @object is JToken;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			if (@object is JValue jvalue)
			{
				yield return new KeyValuePair<string, object>(prefix, jvalue.Value);
				yield break;
			}

			if (@object is JArray jarray)
			{
				for (int i = 0; i < jarray.Count; ++i)
				{
					var adjustedPrefix = $"{prefix}[{i}]";
					foreach (var val in objectFlattener.FlattenObject(adjustedPrefix, jarray[i]))
						yield return val;
				}

				yield break;
			}

			if (@object is JObject jobject)
			{
				foreach (var property in jobject)
				{
					var adjustedPrefix = $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ".")}{property.Key}";
					foreach (var val in objectFlattener.FlattenObject(adjustedPrefix, property.Value))
						yield return val;
				}

				yield break;
			}

			throw new NotSupportedException($"Don't know how to flatten objects of type '{@object.GetType()}'");
		}
	}
}
