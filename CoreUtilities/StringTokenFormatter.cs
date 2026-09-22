using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StringTokenFormatter;

namespace Ensur.Core.Utilities
{
    /// <summary>
    /// Uses https://github.com/andywilsonuk/StringTokenFormatter to parse values in strings
    /// </summary>
    public static class StringFormatter
    {
        /// <summary>
        /// Takes a IEnumerable of key/value pairs and formats a string with them looking for the keys in curly brackets {}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="values"></param>
        /// <example>It would replace {this} and {alsoThis} with keys of the same name</example>
        /// <returns></returns>
        public static string FormatDict<T>(this string input, IEnumerable<KeyValuePair<string, T>> values)
        {
            return input.FormatDictionary<T>(values);
        }


        public static string ParseTokens(this string input, IEnumerable<KeyValuePair<string, string>> values)
        {
            return input.FormatDictionary<string>(values);
        }

        /// <summary>
        /// Parses the string and lookes for property names of the passed object in curly braces
        /// </summary>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <example>Any properties that match names like {ThisProperty} or {ThatProperty} would be replaced</example>
        /// <returns></returns>
        public static string FormatObject(this string input, object value)
        {
            var rslt = input.FormatToken(value);
            return rslt;
        }

        /// <summary>
        /// Parses the string and looks for a single value in curly braces and replaces it with the passed property value.
        /// </summary>
        /// <param name="input">The string containing the value in curlty braces: 'This is going to {ReplaceMe}'</param>
        /// <param name="TokenName">The name of the value in curly brackets</param>
        /// <param name="value">The value to put in the curly braces</param>
        /// <returns></returns>
        public static string ReplaceToken(this string input,string TokenName, string value)
        {
            return input.FormatToken(TokenName, value);
        }
    }
}
