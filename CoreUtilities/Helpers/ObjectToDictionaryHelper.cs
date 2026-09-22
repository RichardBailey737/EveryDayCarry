using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Helpers
{
    /// <summary>
    /// Series of extension methods that manipulate and copy items between dictionaries.
    /// </summary>
    public static class ObjectToDictionaryHelper
    {

        /// <summary>
        /// Merges two dictionaries together.  The supplied dictionary is considered the "overriding" dictionary.  
        /// </summary>
        /// <param name="source">Dictionary to copy into</param>
        /// <param name="OverridingDictionary">Dictionary to add properties from.  Any properties in this dictionary will override properites in the source.</param>
        public static void Merge(this Dictionary<string, object> source, Dictionary<string, object> OverridingDictionary)
        {
            OverridingDictionary.ToList().ForEach(kvp => source[kvp.Key] = kvp.Value);
        }

        /// <summary>
        /// Merges two dictionaries together.  The supplied dictionary is considered the "overriding" dictionary.  
        /// </summary>
        /// <param name="source">Dictionary to copy into</param>
        /// <param name="OverridingDictionary">Dictionary to add properties from.  Any properties in this dictionary will override properites in the source.</param>
        public static void Merge(this Dictionary<string, string> source, Dictionary<string, string> OverridingDictionary)
        {
            OverridingDictionary.ToList().ForEach(kvp => source[kvp.Key] = kvp.Value);
        }
        /// <summary>
        /// Merges two dictionaries together.  The supplied dictionary is considered the "overriding" dictionary.  
        /// </summary>
        /// <param name="source">Dictionary to copy into</param>
        /// <param name="OverridingDictionary">Dictionary to add properties from.  Any properties in this dictionary will override properites in the source.</param>
        public static void Merge(this Dictionary<string, object> source, Dictionary<string, string> OverridingDictionary)
        {
            OverridingDictionary.ToList().ForEach(kvp => source[kvp.Key] = kvp.Value);
        }

        /// <summary>
        /// Merges two dictionaries together.  The supplied dictionary is considered the "overriding" dictionary.  
        /// </summary>
        /// <param name="source">Dictionary to copy into</param>
        /// <param name="OverridingDictionary">Dictionary to add properties from.  Any properties in this dictionary will override properites in the source.</param>
        public static void Merge(this Dictionary<string, string> source, Dictionary<string, object> OverridingDictionary)
        {
            OverridingDictionary.ToList().ForEach(kvp => source[kvp.Key] = kvp.Value.ToString());
        }

        /// <summary>
        /// Converts an object to a dictionary by using reflection to go through it's properties, and adding them as dictionary string, object key value pairs.
        /// </summary>
        /// <param name="source">The object to convert</param>
        /// <returns>Dictionary of key/value pairs with all the values from the object</returns>
        public static Dictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        /// <summary>
        /// Converts an object to a dictionary by using reflection to go through it's properties, and adding them as dictionary string, object key value pairs.
        /// </summary>
        /// <param name="source">The object to convert</param>
        /// <returns>Dictionary of key/value pairs with all the values from the object</returns>
        public static Dictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
                AddPropertyToDictionary<T>(property, source, dictionary);
            return dictionary;
        }

        public static Dictionary<string, T> ToDictionary<T>(this object source, string ExcludedProperties)
        {
            if (source == null)
            {
                ThrowExceptionWhenSourceArgumentIsNull();
            }

            if (ExcludedProperties == null) return source.ToDictionary<T>();

            var excluded = ExcludedProperties.ToUpper().Split(',');

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                if (!excluded.Contains(property.Name.Trim().ToUpper())) AddPropertyToDictionary<T>(property, source, dictionary);
            }

            return dictionary;
        }

        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            object value = property.GetValue(source);
            if (IsOfType<T>(value))
                dictionary.Add(property.Name, (T)value);
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
        }



    }


}
