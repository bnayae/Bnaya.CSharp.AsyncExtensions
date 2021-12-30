using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Error formatting options
    /// </summary>
    [Flags]
    public enum ErrorFormattingOption
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// replace duplicate path (namespace / class) with a sign (by default '-')
        /// </summary>
        FormatDuplication = 1,
        /// <summary>
        /// The include line number
        /// </summary>
        IncludeLineNumber = FormatDuplication * 2,
        /// <summary>
        /// The include full unformatted details
        /// </summary>
        IncludeFullUnformattedDetails = IncludeLineNumber * 2,
        /// <summary>
        /// The default
        /// </summary>
        Default = IncludeLineNumber | FormatDuplication,
    }
}
