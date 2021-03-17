using System;
using System.Collections.Generic;
using System.Linq;

namespace BorsukSoftware.ObjectFlattener
{
	/// <summary>
	/// Standard implementation of <see cref="IObjectFlattener"/>
	/// </summary>
	public class ObjectFlattener : IObjectFlattener
	{
		#region Data Model

		/// <summary>
		/// Get the set of plugins which will perform the actual flattening
		/// </summary>
		/// <remarks>The order is important here, the first plugin which matches will be used</remarks>
		public IList<IObjectFlatteningPlugin> Plugins { get; private set; } = new List<IObjectFlatteningPlugin>();

		public NoAvailablePluginBehaviour NoAvailablePluginBehaviour { get; set; } = NoAvailablePluginBehaviour.Throw;

		#endregion

		#region IObjectFlattener Members

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(string prefix, object sourceObject)
		{
			var plugin = this.Plugins.FirstOrDefault(p => p.CanHandle(prefix, sourceObject));
			if (plugin == null)
			{
				switch( this.NoAvailablePluginBehaviour )
				{
					case NoAvailablePluginBehaviour.Throw:
						throw new InvalidOperationException($"Unable to find a plugin to handle value for '{prefix}' - {sourceObject}");

					case NoAvailablePluginBehaviour.Ignore:
						return Enumerable.Empty<KeyValuePair<string, object>>();

					case NoAvailablePluginBehaviour.ReturnValue:
						return new[] { new KeyValuePair<string, object>(prefix, sourceObject) };

					default:
						throw new NotSupportedException($"Unsupported no available plugin behaviour flag - '{this.NoAvailablePluginBehaviour}'");
				}
			}

			return plugin.FlattenObject(this, prefix, sourceObject);
		}

		#endregion
	}
}
