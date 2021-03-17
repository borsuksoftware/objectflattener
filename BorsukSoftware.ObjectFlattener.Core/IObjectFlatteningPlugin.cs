using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener
{
	public interface IObjectFlatteningPlugin
	{
		bool CanHandle(string prefix, object @object);

		IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object);
	}
}
