using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Error formatting options
    /// </summary>
    public enum ErrorFormattingOption
    {
        /// <summary>
        /// The default
        /// </summary>
        Default,
        /// <summary>
        /// replace duplicate path (namespace / class) with dots
        /// </summary>
        DotForDuplicate
    }
}
