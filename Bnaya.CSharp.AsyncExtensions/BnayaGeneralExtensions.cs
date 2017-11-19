using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Task Extensions
    /// </summary>
    public static class BnayaGeneralExtensions
    {
        #region Plural

        /// <summary>
        /// Plurals the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IEnumerable<T> Plural<T>(this T item)
        {
            yield return item;
        }

        #endregion // Plural
    }
}

