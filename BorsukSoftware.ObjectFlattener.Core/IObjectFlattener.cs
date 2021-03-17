using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectFlattener
{
	public interface IObjectFlattener
	{
		IEnumerable<KeyValuePair<string, object>> FlattenObject(string prefix, object sourceObject);
	}
}
