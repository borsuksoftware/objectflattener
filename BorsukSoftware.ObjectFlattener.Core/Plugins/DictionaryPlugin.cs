using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Standard plugin to process instances of <see cref="IDictionary{TKey, TValue}"/>
	/// </summary>
	/// <remarks>As each dictionary might have different requirements for how it's flattened (e.g. creating a suitable text key for a string or integer is fairly 
	/// simple, for more complex types, or where some numerical rouding might be required, it's less clear that there is a single correct way), the framework is
	/// structured that a user can provide a function to choose their own implementation</remarks>
	/// <typeparam name="TKey"></typeparam>
	public class DictionaryPlugin<TKey> : IObjectFlatteningPlugin
	{
		#region Data Model

		/// <summary>
		/// Get the func which will be used to generate the flattened key for each entry
		/// </summary>
		public Func<TKey, string> KeyFunc { get; private set; }

		#endregion

		public DictionaryPlugin(Func<TKey, string> keyFunc)
		{
			if (keyFunc == null)
				throw new ArgumentNullException(nameof(keyFunc));

			this.KeyFunc = keyFunc;
		}

		#region IObjectFlatteningPlugin Members

		public bool CanHandle(string prefix, object @object)
		{
			var objectType = @object?.GetType();
			if (objectType != null)
			{
				var interfaces = objectType.GetInterfaces().
					Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).
					Where(i => i.GetGenericArguments()[0] == typeof(TKey)).
					ToList();

				if (interfaces.Any())
					return true;
			}

			return false;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			var keyType = @object.GetType().GetInterfaces().
				Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).
				Where(i => i.GetGenericArguments()[0] == typeof(TKey)).
				Select(i => i.GetGenericArguments()[1]).
				First();

			var filterMethod = typeof(DictionaryPlugin<TKey>).GetMethod(nameof(FlattenDictionary), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var filterMethodWithType = filterMethod.MakeGenericMethod(keyType);

			return (IEnumerable<KeyValuePair<string, object>>)filterMethodWithType.Invoke(this, new object[] { objectFlattener, prefix, @object });
		}

		private IEnumerable<KeyValuePair<string, object>> FlattenDictionary<TValue>(IObjectFlattener objectFlattener, string prefix, IDictionary<TKey, TValue> dictionary)
		{
			var output = dictionary.
				Select(pair => new
				{
					adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ".")}{this.KeyFunc(pair.Key)}",
					pair
				}).
				SelectMany(tuple => objectFlattener.FlattenObject(tuple.adjustedName, tuple.pair.Value));

			return output;
		}

		#endregion
	}
}
