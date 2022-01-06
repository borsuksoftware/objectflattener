using System;
using System.Collections.Generic;
using System.Linq;

namespace BorsukSoftware.ObjectFlattener.Plugins
{
	/// <summary>
	/// Standard plugin which performs standard flattening, allowing for leaf types and expanding all other objects by default
	/// </summary>
	/// <remarks>This plugin can be treated as providing the standard, default behaviour for object flattening, including:
	/// 
	/// <list type="bullet">
	/// <item>Handling leaf nodes - stopping further object inspection</item>
	/// <item>Expanding objects with child properties</item>
	/// </list>
	/// 
	/// <para>If the default behaviour isn't desired for whatever reason, then custom plugins can be created to perform that
	/// functionality. </para>
	/// </remarks>
	public class StandardPlugin : IObjectFlatteningPlugin
	{
		#region Data Model

		/// <summary>
		/// Get / set whether or not to handle instances of <see cref="Nullable{T}"/> as a leaf node
		/// </summary>
		public bool ProcessNullableTypesAsLeafTypes { get; set; } = true;

		/// <summary>
		/// Get / set whether or not to process enums as leaf types
		/// </summary>
		public bool ProcessEnumTypesAsLeafTypes { get; set; } = true;

		/// <summary>
		/// Get the set of leaf types
		/// </summary>
		/// <remarks>This can be altered on demand</remarks>
		public ISet<Type> LeafTypes { get; private set; } = new HashSet<Type>();

		/// <summary>
		/// Get / set whether or not to extract properties from objects
		/// </summary>
		public bool ProcessProperties { get; set; } = true;

		/// <summary>
		/// Get / set whether or not to extract fields from objects
		/// </summary>
		public bool ProcessFields { get; set; } = false;

		/// <summary>
		/// Get / set whether or not to treat arrays as leaf objects
		/// </summary>
		/// <remarks>To operate in split mode, use <see cref="Plugins.ArrayPlugin"/> with a higher priority than this</remarks>
		public bool ProcessArraysAsLeafType { get; set; } = true;

		#endregion

		public StandardPlugin()
		{
			this.LeafTypes.Add(typeof(DateTime));
			this.LeafTypes.Add(typeof(DateTimeOffset));
			this.LeafTypes.Add(typeof(TimeSpan));
			this.LeafTypes.Add(typeof(Guid));
			this.LeafTypes.Add(typeof(IntPtr));
			this.LeafTypes.Add(typeof(UIntPtr));
			this.LeafTypes.Add(typeof(System.Numerics.BigInteger));
			this.LeafTypes.Add(typeof(System.Numerics.Complex));
			this.LeafTypes.Add(typeof(string));
			this.LeafTypes.Add(typeof(bool));
			this.LeafTypes.Add(typeof(sbyte));
			this.LeafTypes.Add(typeof(short));
			this.LeafTypes.Add(typeof(int));
			this.LeafTypes.Add(typeof(long));
			this.LeafTypes.Add(typeof(byte));
			this.LeafTypes.Add(typeof(ushort));
			this.LeafTypes.Add(typeof(uint));
			this.LeafTypes.Add(typeof(ulong));
			this.LeafTypes.Add(typeof(float));
			this.LeafTypes.Add(typeof(double));
			this.LeafTypes.Add(typeof(decimal));
		}

		public bool CanHandle(string prefix, object @object)
		{
			return true;
		}

		public IEnumerable<KeyValuePair<string, object>> FlattenObject(IObjectFlattener objectFlattener, string prefix, object @object)
		{
			if (@object == null)
			{
				yield return new KeyValuePair<string, object>(prefix, @object);
				yield break;
			}

            var objectType = @object.GetType();
            if (this.LeafTypes.Contains(objectType) ||
                (this.ProcessEnumTypesAsLeafTypes && objectType.IsEnum) ||
                (objectType.IsArray && this.ProcessArraysAsLeafType))
            {
                yield return new KeyValuePair<string, object>(prefix, @object);
                yield break;
            }

            if (this.ProcessNullableTypesAsLeafTypes &&
				objectType.IsGenericType &&
				objectType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
				this.LeafTypes.Contains(objectType.GetGenericArguments()[0]))
			{
				yield return new KeyValuePair<string, object>(prefix, @object);
				yield break;
			}

			if (this.ProcessProperties)
			{
				var properties = objectType.
					GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).
					Where(p => p.CanRead).
					Where(p => p.GetIndexParameters().Length == 0);

				foreach (var property in properties)
				{
					var propertyValue = property.GetValue(@object);
					var adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ".")}{property.Name}";
					foreach (var value in objectFlattener.FlattenObject(adjustedName, propertyValue))
						yield return value;
				}
			}

			if (this.ProcessFields)
			{
				var fields = objectType.
					GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

				foreach (var field in fields)
				{
					var fieldValue = field.GetValue(@object);
					var adjustedName = $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ".")}{field.Name}";
					foreach (var value in objectFlattener.FlattenObject(adjustedName, fieldValue))
						yield return value;
				}
			}
		}
	}
}
