using JBSnorro;
using JBSnorro.Extensions;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Reflection;

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
            return ParameterView.FromDictionary(result);
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
            return ParameterView.FromDictionary(result);
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
        
        
    }

}