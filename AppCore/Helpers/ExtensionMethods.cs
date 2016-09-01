using System;
using System.Collections.Generic;
using System.Linq;

namespace PIReplay.Core.Helpers
{
    public static class ExtensionMethods
    {
  
        /// <summary>
        ///     Helper methods for the lists.
        /// </summary>
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        ///     Convert a date to a human readable ISO datetime format. ie. 2012-12-12 23:01:12
        ///     this method must be put in a static class. This will appear as an available function
        ///     on every datetime objects if your static class namespace is declared.
        /// </summary>
        public static string ToIsoReadable(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH':'mm':'ss");
        }


        /// <summary>
        ///     Put a string between double quotes.
        /// </summary>
        /// <param name="value">Value to be put between double quotes ex: foo</param>
        /// <returns>double quoted string ex: "foo"</returns>
        public static string PutIntoQuotes(this string value)
        {
            return "\"" + value + "\"";
        }
    }
}
