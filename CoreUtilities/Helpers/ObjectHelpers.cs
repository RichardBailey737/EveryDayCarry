using DocumentFormat.OpenXml.Wordprocessing;
using Ensur.Core.Utilities.Database;
using PetaPoco;
using PetaPoco.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Helpers
{

    /// <summary>
    /// Extension method that copies properties from one objecto to another.
    /// </summary>
    public static class ObjectHelpers
    {
        /// <summary>
        /// Copies all the properties from one object to another. Only copies properties that have the same name.
        /// </summary>
        /// <typeparam name="T">Type of the source object</typeparam>
        /// <typeparam name="TU">Type of the destination object</typeparam>
        /// <param name="source">Source object (this)</param>
        /// <param name="dest">Destination object to copy properties to</param>
        /// <param name="IncludedColumns">Comma delimited list of columns to include (case sensitive)</param>
        /// <param name="ExcludedColumns">Comma delimited list of columns to exclude (case sensitive)</param>
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest, string IncludedColumns, string ExcludedColumns)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead && !Attribute.IsDefined(x, typeof(IgnoreAttribute))).ToList();

            if (!String.IsNullOrEmpty(IncludedColumns))
            {
                var included = IncludedColumns.Split(',').Select(s => s.Trim().ToUpper());
                sourceProps = sourceProps.Where(prop => included.Contains(prop.Name.ToUpper())).ToList();
            }

            if (!String.IsNullOrEmpty(ExcludedColumns))
            {
                var excluded = ExcludedColumns.Split(',').Select(s => s.Trim().ToUpper());
                sourceProps = sourceProps.Where(prop => !excluded.Contains(prop.Name.ToUpper())).ToList();
            }

            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }
            }
        }

        public static void CopyPropertiesToPetaPoco<T, TU>(this T source, TU dest)
        {
            var database = new PocoDB();
            var pocoData = PocoData.ForType(typeof(TU), database.DefaultMapper);

            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead && !Attribute.IsDefined(x, typeof(IgnoreAttribute))).ToList();

            foreach (var sourceProp in sourceProps)
            {

                if (pocoData.Columns.Any(c => c.Key.ToUpper() == sourceProp.Name.ToUpper()))
                {
                    var property = pocoData.Columns.SingleOrDefault(c => c.Key.ToUpper() == sourceProp.Name.ToUpper());
                    try
                    {
                        if (property.Value.PropertyInfo.PropertyType == sourceProp.PropertyType)
                        {
                            property.Value.SetValue(dest, sourceProp.GetValue(source, null));
                        }
                        else
                        {
                            var initval = sourceProp.GetValue(source, null);
                            if (initval != null)
                            {
                                var sourceVal = Convert.ToString(initval);

                                property.Value.SetValue(dest, sourceVal.ConvertTo(property.Value.PropertyInfo.PropertyType));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error copying property {sourceProp.Name} to {property.Value.ColumnName} {sourceProp.PropertyType.Name}-{property.Value.PropertyInfo.PropertyType.Name} : {ex.Message}");
                    }

                }
            }
        }

        public static TU ConvertObject<T, TU>(this T source)
        {
            var database = new PocoDB();
            var pocoData = PocoData.ForType(typeof(TU), database.DefaultMapper);
            var dest = Activator.CreateInstance<TU>();
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead && !Attribute.IsDefined(x, typeof(IgnoreAttribute))).ToList();

            foreach (var sourceProp in sourceProps)
            {

                if (pocoData.Columns.Any(c => c.Key.ToUpper() == sourceProp.Name.ToUpper()))
                {
                    var property = pocoData.Columns.SingleOrDefault(c => c.Key.ToUpper() == sourceProp.Name.ToUpper());
                    property.Value.SetValue(dest, sourceProp.GetValue(source, null));

                }
            }
            return dest;
        }

        public static List<TU> ConvertObject<T, TU>(this List<T> source)
        {
            List<TU> dest = new List<TU>();
            foreach (var item in source)
            {
                dest.Add(item.ConvertObject<T, TU>());
            }

            return dest;
        }

        /// <summary>
        /// Creates an instance of the source class and copies all the values from the source to the destination.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(this T source) where T : class
        {
            var dest = Activator.CreateInstance<T>();
            source.CopyPropertiesTo(dest, null, null);
            return dest;
        }

        /// <summary>
        /// Returns a strongly typed property by property name so it can be loaded dynamically
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="source"></param>
        /// <param name="PropertyName">Name of the property to return (case insensitive)</param>
        /// <returns></returns>
        public static TReturn GetPropertyValue<T, TReturn>(this T source, string PropertyName)
        {
            var prop = typeof(T).GetProperties().SingleOrDefault(p => p.Name.ToUpper() == PropertyName.ToUpper());
            if (prop == null)
            {
                throw new Exception("Invalid property name");
            }
            else
            {
                if (source == null) return default(TReturn);
                return (TReturn)prop.GetValue(source);
            }
        }

        /// <summary>
        /// Converts a 1/0 integer to bool, 1 being true and anything else false.
        /// </summary>
        /// <param name="num"></param>
        /// <returns>true if the number = 1</returns>
        public static bool ToBool(this int num)
        {
            if (num == 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Converts a 1/0/null integer to bool, 1 being true and anything else false.
        /// </summary>
        /// <param name="num"></param>
        /// <returns>true if the number = 1</returns>
        public static bool ToBool(this int? num)
        {
            if (num.HasValue && num == 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Converts a date/time to the standard long date format dd-MMM-yyyy hh:mm:ss tt
        /// </summary>
        /// <param name="dateField"></param>
        /// <returns></returns>
        public static string ToStandardDate(this DateTime? dateField)
        {
            return string.Format("{0:dd-MMM-yyyy hh:mm:ss tt}", dateField);
        }

        /// <summary>
        /// Converts a date/time to the standard long date/time format dd-MMM-yyyy hh:mm:ss tt
        /// </summary>
        /// <param name="dateField"></param>
        /// <returns></returns>
        public static string ToStandardDate(this DateTime dateField)
        {
            return string.Format("{0:dd-MMM-yyyy hh:mm:ss tt}", dateField);
        }

        /// <summary>
        /// Converts a date/time to the standard short date format dd-MMM-yyyy
        /// </summary>
        /// <param name="dateField"></param>
        /// <returns></returns>
        public static string ToStandardShortDate(this DateTime? dateField)
        {
            return string.Format("{0:dd-MMM-yyyy}", dateField);
        }

        /// <summary>
        /// Converts a date/time to the standard short date format dd-MMM-yyyy
        /// </summary>
        /// <param name="dateField"></param>
        /// <returns></returns>
        public static string ToStandardShortDate(this DateTime dateField)
        {
            return string.Format("{0:dd-MMM-yyyy}", dateField);
        }

        /// <summary>
        /// Converts a string value to an enumeration value of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string val) where T : System.Enum
        {
            Regex r = new Regex("[^0-9a-zA-Z]+");
            return (T)Enum.Parse(typeof(T), r.Replace(val, "_"), true);
        }

        /// <summary>
        /// Converts a string value to an enumeration value of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this int val) where T : System.Enum
        {
            return (T)Enum.Parse(typeof(T), val.ToString(), true);
        }

        /// <summary>
        /// Simple shortcut utility that checks to see if the value is null or empty
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool IsNotNull(this string val)
        {
            return !string.IsNullOrEmpty(val);
        }

        /// <summary>
        /// Takes a string value and attemps to execute a "parse" function on the destination type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this string val)
        {
            Type destiny = typeof(T);

            if (destiny.IsGenericType && destiny.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                destiny = Nullable.GetUnderlyingType(destiny);
            }


            // See if we can parse
            try
            {
                return (T)destiny.InvokeMember("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            }
            catch { }

            // See if we can convert
            try
            {
                Type convertType = typeof(Convert);
                return (T)convertType.InvokeMember("To" + destiny.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            }
            catch { }

            // Give up
            return default(T);
        }

        /// <summary>
        /// Converts an object to any other type (accounting for nullable objects)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T ConvertObjTo<T>(object val)
        {
            Type destiny = typeof(T);

            if (destiny.IsGenericType && destiny.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                destiny = Nullable.GetUnderlyingType(destiny);
            }


            // See if we can parse
            try
            {
                return (T)destiny.InvokeMember("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            }
            catch { }

            // See if we can convert
            try
            {
                Type convertType = typeof(Convert);
                return (T)convertType.InvokeMember("To" + destiny.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            }
            catch { }

            // Give up
            return default(T);
        }

        /// <summary>
        /// Takes a string value and attemps to execute a "parse" function on the destination type.
        /// </summary>
        /// <returns></returns>
        public static object ConvertTo(this string val, Type DestinationType)
        {
            Type destiny = DestinationType;

            if (destiny.IsGenericType && destiny.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                destiny = Nullable.GetUnderlyingType(destiny);
            }

            if (destiny == typeof(string)) return val.ToString();

            if (destiny == typeof(bool))
            {
                val = val.Replace("1", "true").Replace("0", "false");
            }

            // See if we can parse
            return destiny.InvokeMember("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });


            //// See if we can convert
            //try
            //{
            //    Type convertType = typeof(Convert);
            //    return convertType.InvokeMember("To" + destiny.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            //}
            //catch { }

            //// Give up
            //return val;
        }

        /// <summary>
        /// Splits a string into pieces that are no more than "Length" long by the nearest space. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">The length to split the string by.</param>
        /// <returns></returns>
        public static IEnumerable<String> SplitByNearestSpace(this String value, int length)
        {
            if (String.IsNullOrEmpty(value))
                yield break;

            string remainingVal = value;
            string segment = "";
            int lastSpace = 0;
            while (remainingVal.Length > 0)
            {
                if (remainingVal.Length > length)
                {
                    segment = remainingVal.Substring(0, length);
                    lastSpace = segment.LastIndexOf(" ");
                    if (lastSpace > 0)
                    {
                        segment = segment.Substring(0, lastSpace);
                        remainingVal = remainingVal.Substring(lastSpace + 1);
                    }
                    else
                    {
                        remainingVal = remainingVal.Substring(length);
                    }
                }
                else
                {
                    segment = remainingVal;
                    remainingVal = "";
                }
                yield return segment;
            }
        }

        public static bool IsNumeric(this string value) => double.TryParse(value, out _);

        public static bool IsNumericType(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNumericType(this Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsValidEmailAddress(this string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string Shorten(this string StringToShorten, int ShortenToCharacters)
        {
            if (!String.IsNullOrEmpty(StringToShorten) && StringToShorten.Length > ShortenToCharacters)
            {
                return StringToShorten.Substring(0, ShortenToCharacters) + "...";
            }
            else
                return StringToShorten;

        }

        /// <summary>
        /// Utility function to convert an object to something more readable.  Good for error debugging
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToJSONString(this object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }


        /// <summary>
        /// Shortcut to see if a value is in a series of values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool InList<T>(this T obj, params T[] values) where T : struct
        {
            foreach (var val in values)
            {
                if (obj.Equals(val)) return true;
            }
            return false;
        }


        public static T GetOrDefault<T>(this T[] array, int index, T defaultValue = default)
        {
            if (array == null) return defaultValue;
            return (index >= 0 && index < array.Length) ? array[index] : defaultValue;
        }


    }
}
