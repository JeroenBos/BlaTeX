#nullable enable
using JBSnorro;
using JBSnorro.Extensions;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Reflection;
using JBSnorro.Diagnostics;
using System.Runtime.CompilerServices;

namespace BlaTeX
{
	public static class ParameterViewExtensions
	{
		/// <summary> Extracts a new parameter view with only the specified keys, omitting them if not present. </summary>
		public static ParameterView FilterKeys(this ParameterView parameters, params string[] keys)
		{
			var result = new Dictionary<string, object>();
			foreach (var key in keys)
			{
				if (parameters.TryGetValue<object>(key, out var value))
					result.Add(key, value);
			}
			return ParameterView.FromDictionary(result!);
		}
		/// <summary> Extracts a new parameter view with only the specified keys, omitting them if not present. </summary>
		public static ParameterView FilterKeys(this ParameterView parameters, Type baseType)
		{
			var parameterNames = GetBaseTypeKeys(baseType);
			return parameters.FilterKeys(parameterNames.ToArray());
		}
		/// <summary> Extracts a new parameter view with only the specified keys, throwing if a key is not present. </summary>
		public static ParameterView Select(this ParameterView parameters, params string[] keys)
		{
			var result = new Dictionary<string, object>();
			var dict = parameters.ToDictionary();
			foreach (var key in keys)
			{
				result[key] = dict[key];
			}
			return ParameterView.FromDictionary(result!);
		}
		/// <summary> Extracts a new parameter view with only the specified keys, throwing if a key is not present. </summary>
		public static ParameterView Select(this ParameterView parameters, Type baseType)
		{
			var parameterNames = GetBaseTypeKeys(baseType);
			return parameters.Select(parameterNames.ToArray());
		}
		/// <summary> Gets the properties annotated with [Parameter] on the specified base type and its base types. </summary>
		private static IEnumerable<string> GetBaseTypeKeys(Type baseType)
		{
			const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
			return baseType.GetProperties(flags)
						   .Where(property => property.HasAttribute<ParameterAttribute>())
						   .Select(property => property.Name);
		}
		/// <summary> Gets whether the specified parameters contains the specified parameter name. </summary>
		public static bool Contains(this ParameterView parameters, string parameterName)
		{
			return parameters.TryGetValue<object>(parameterName, out var _);
		}
		/// <summary> Creates a new parameter view with the specified parameter name and value. Overrides if already exists. </summary>
		public static ParameterView With(this ParameterView parameters, string parameterName, object? value)
		{
			Contract.Requires(parameterName != null);

			var result = parameters.ToDictionary().ToDictionary();
			result[parameterName] = value!;
			return ParameterView.FromDictionary(result!);
		}
		/// <summary> Creates a new parameter view excluding the specified parameter names if present. </summary>
		public static ParameterView Without(this ParameterView parameters, params string[] parameterNamesToExclude)
		{
			Contract.Requires(parameterNamesToExclude != null);

			var result = parameters.ToDictionary().ToDictionary();
			foreach (var exclude in parameterNamesToExclude)
			{
				result.Remove(exclude);
			}
			return ParameterView.FromDictionary(result!);
		}
		/// <summary> Get all parameter properties on the specified type. </summary>
		public static IEnumerable<PropertyInfo> GetBlazorParameters(this Type componentType)
		{
			if (componentType == null)
				throw new ArgumentNullException(nameof(componentType));

			return componentType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public)
								.Where(p => p.HasAttribute<ParameterAttribute>());
		}
		/// <summary> Get the names of all parameters on the specified type. </summary>
		public static IEnumerable<string> GetBlazorParameterNames(this Type componentType)
		{
			return componentType.GetBlazorParameters().Select(p => p.Name);
		}
		/// <summary> Gets whether the specified parameter view a subset of the specified names. </summary>
		public static bool ContainsOnly(this ParameterView parameters, params string[] names)
		{
			var d = parameters.ToDictionary().ToDictionary();
			foreach (var name in names)
				d.Remove(name);

			return d.Count == 0;
		}
		/// <summary> Asserts that the specified parameter view a subset of the specified names. </summary>
		[System.Diagnostics.Conditional("DEBUG")]
		public static void AssertContainsOnly(this ParameterView parameters, params string[] names)
		{
			var d = parameters.ToDictionary().ToDictionary();
			foreach (var name in names)
				d.Remove(name);

			if (d.Count == 0)
				return;

			throw new ContractException("Unrecognized parameters specified: " + d.Select(pair => pair.Key).Join(", "));
		}
		[System.Diagnostics.Conditional("DEBUG")]
		public static void AssertMissingOrNotNull(this ParameterView parameters, params string[] names)
		{
			foreach (string name in names)
			{
				if (parameters.TryGetValue<object>(name, out var argument))
					if (argument is null)
						throw new ArgumentNullException(name);
			}
		}
		/// <summary>
		/// Asserts that after setting the parameter if present, the value would not be null.
		/// </summary>
		[System.Diagnostics.Conditional("DEBUG")]
		public static void AssertPresent<T>(this ParameterView parameters, T? currentValue, string name, string message = "Mandatory argument missing") where T : class
		{
			if (parameters.TryGetValue<T?>(name, out var argument))
			{
				if (argument is null)
					throw new ArgumentNullException(name);
			}
			else if (currentValue is null)
			{
				throw new ArgumentException(message, name);
			}
		}
		public static ParameterView ToParameterView(this IDictionary<string, object> dictionary) => ParameterView.FromDictionary(dictionary!);
		public static ParameterView Create(params (string, object)[] parameters)
		{
			return parameters.Select(ToKeyValuePair)
							 .ToDictionary()
							 .ToParameterView();
		}
		public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this (TKey, TValue) tuple) => new KeyValuePair<TKey, TValue>(tuple.Item1, tuple.Item2);
	}
}
