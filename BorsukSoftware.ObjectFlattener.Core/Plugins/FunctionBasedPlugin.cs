using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Function based plugin which can be used to easily provide custom logic for object flattening
	/// </summary>
	public class FunctionBasedPlugin : IObjectFlatteningPlugin
	{
		#region Data Model

		public Func<string, object, bool> CanHandleFunc { get; private set; }

		public Func<IObjectFlattener, string, object, IEnumerable<KeyValuePair<string, object>>> FlattenObjectFunc { get; private set; }

		#endregion

		public FunctionBasedPlugin(Func<string, object, bool> canHandleFunc, Func<IObjectFlattener, string, object, IEnumerable<KeyValuePair<string, object>>> flattenObjectFunc)
		{
			if (canHandleFunc == null)
				throw new ArgumentNullException(nameof(canHandleFunc));

			if (flattenObjectFunc == null)
				throw new ArgumentNullException(nameof(flattenObjectFunc));

			this.CanHandleFunc = canHandleFunc;
			this.FlattenObjectFunc = flattenObjectFunc;
		}

		#region IObjectFlatteningPlugin Members

		public bool CanHandle(string prefix, object @object)
		{
			return this.CanHandleFunc(prefix, @object);
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			return this.FlattenObjectFunc(objectFlattener, prefix, @object);
		}

		#endregion
	}
}
