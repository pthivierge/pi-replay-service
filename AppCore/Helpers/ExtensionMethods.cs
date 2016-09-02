#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
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
