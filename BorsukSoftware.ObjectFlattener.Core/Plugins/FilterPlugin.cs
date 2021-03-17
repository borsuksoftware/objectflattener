using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Helper plugin which can be used to reduce the scope of an existing plugin
	/// </summary>
	/// <remarks>This can be very useful if there's a requirement to handle similar objects in a different
	/// way, e.g. in some use-cases it may be useful to handle different types of arrays differently and
	/// if the desired behaviour per type can be handled by existing plugins, then this class can be used
	/// to avoid the need to duplicate that logic</remarks>
	public class FilterPlugin : IObjectFlatteningPlugin
	{
		#region Data Model

		public Func<string,object,bool> CanHandleFunc { get; private set; }
		public IObjectFlatteningPlugin UnderlyingPlugin { get; private set; }

		#endregion

		public FilterPlugin( IObjectFlatteningPlugin underlyingPlugin,
			Func<string,object,bool> canHandleFunc )
		{
			if (underlyingPlugin == null)
				throw new ArgumentNullException(nameof(underlyingPlugin));

			if (canHandleFunc == null)
				throw new ArgumentNullException(nameof(canHandleFunc));

			this.CanHandleFunc = canHandleFunc;
			this.UnderlyingPlugin = underlyingPlugin;
		}

		public bool CanHandle(string prefix, object @object)
		{
			return this.CanHandleFunc(prefix, @object);
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			return this.UnderlyingPlugin.FlattenObject(objectFlattener, prefix, @object);
		}
	}
}
