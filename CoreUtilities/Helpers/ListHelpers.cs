using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Ensur.Core.Utilities.Helpers
{

    /// <summary>
    /// A collection of extension methods for lists.
    /// </summary>
    public static class ListHelpers
    {
 

        /// <summary>
        /// Checks a list to see if an object of that type and value exists.  Automatically ignores cases on strings.  Why did I create a JUST BARELY shorter way to check if a value exists in a list?  Mostly for string comparisons.
        /// </summary>
        /// <typeparam name="ListType">The object type contained by the list</typeparam>
        /// <param name="list">The list this is executing on</param>
        /// <param name="Property">The property to check</param>
        /// <param name="PropertyValue">The value to compare against</param>
        /// <returns>True if the item is contained in the list</returns>
        public static bool Exists<ListType>(this List<ListType> list, System.Linq.Expressions.Expression<Func<ListType, object>> Property, object PropertyValue)
        {
            System.Linq.Expressions.LambdaExpression lambda = (System.Linq.Expressions.LambdaExpression)Property;
            System.Linq.Expressions.MemberExpression memberExpression;
            
            if (lambda.Body is System.Linq.Expressions.UnaryExpression)
            {
                System.Linq.Expressions.UnaryExpression unaryExpression = (System.Linq.Expressions.UnaryExpression)(lambda.Body);
                memberExpression = (System.Linq.Expressions.MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (System.Linq.Expressions.MemberExpression)(lambda.Body);
            }
            string PropertyName = ((PropertyInfo)memberExpression.Member).Name;
            var propinfo = typeof(ListType).GetProperty(PropertyName);

            bool rtrn = false;  //Added for debugging purposes
            if (PropertyValue is string)
            {
                rtrn = list.Any(listvalue => String.Equals((propinfo.GetValue(listvalue).ToString() ?? "").Trim(), (PropertyValue.ToString() ?? "").Trim(), StringComparison.OrdinalIgnoreCase));
                return rtrn;
            }
            else
            {
                rtrn = list.Any(listvalue => PropertyValue.Equals(propinfo.GetValue(listvalue)));
                return rtrn;
            }
        }


        /// <summary>
        /// Queries a list and returns a single value where the Property (PropertyName) equals PropertyValue and then returns the ReturnProperty on the single object.  Fails if list has more than one object with that value or if no objects have that value.
        /// </summary>
        /// <typeparam name="ListType">The object type contained by the list</typeparam>
        /// <typeparam name="ValueType">The value type to return</typeparam>
        /// <param name="list">The list this is executing on</param>
        /// <param name="PropertyName">The property in the contained object to query</param>
        /// <param name="PropertyValue">The value to query for</param>
        /// <param name="ReturnProperty">The name of the property to return</param>
        /// <returns>The list item property ReturnProperty that has a property of PropertyName with a value of PropertyValue</returns>
        /// <remarks>Shortcut function.  Can EASILY be done another way but this shrinks the code down.</remarks>
        public static ValueType SingleVal<ListType, ValueType>(this List<ListType> list, string PropertyName, object PropertyValue, string ReturnProperty)
        {
            var propinfo = typeof(ListType).GetProperty(PropertyName);
            var returnProp = typeof(ListType).GetProperty(ReturnProperty);
            if (PropertyValue is string)
            {
                var val = list.SingleOrDefault(li => String.Equals(propinfo.GetValue(li).ToString().Trim(), PropertyValue.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
                if (val == null) throw new ArgumentException($"List does not contain an object where {PropertyName} equals {PropertyValue}");
                return (ValueType)Convert.ChangeType(returnProp.GetValue(val), typeof(ValueType));
            }
            else
            {
                var val = list.SingleOrDefault(li => PropertyValue.Equals(propinfo.GetValue(li)));
                if (val == null) throw new ArgumentException($"List does not contain an object where {PropertyName} equals {PropertyValue}");
                return (ValueType)Convert.ChangeType(returnProp.GetValue(val), typeof(ValueType));
            }
        }
        

    }
}
