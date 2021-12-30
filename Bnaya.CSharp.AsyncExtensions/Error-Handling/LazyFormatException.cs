using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Threading.Tasks.ErrorFormattingOption;

[assembly: InternalsVisibleTo("Bnaya.CSharp.AsyncExtensions.Tests")]
#pragma warning disable CS0618 // Type or member is obsolete

namespace System.Threading.Tasks
{
    /// <summary>
    /// Exception wrapper which do the format of the exception at ToSting timing.
    /// This can reduce compute and memory pressure when the exception send to
    /// semantical logging where it might be filter out and won't be present.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
    public class LazyFormatException: Exception
    {
        private const int MAX_LEN_OF_INNER_SNAP_LINE = 50;
        private const string THROW_PREFIX = "  # Throw";
        private const string START_TASK_TAG = " ~ Start Task ~>";
        private const string START_ASYNC_TAG = " ~ Start ~>\r\n";
        //private static readonly Regex ASYNC_REGEX = new Regex(@"\.(.*)\.<(.*)>d__"); // .{group 0 - any}.<{group 1 = any}>d__
        // "^\s*at = start with 'at ' optional preceding whitespace 
        // (.*\)) = group any until ')'
        private static readonly Regex SYNC_REGEX = new Regex(@"^\s*at (.*\))");

        private static readonly Regex EXCLUDE = new Regex(@"[\W|_]"); // not char, digit  
        private static readonly Regex EXCLUDE_FROM_START = new Regex(@"^[\W|_]"); // not start with char, digit  
        private static readonly Regex ASYNC_REGEX1 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<(?<method>\w+)\>d__[0-9]+.MoveNext\(\)\sin\s(?<file>.*):(?<loc>line\s.*)"); // at {nameespace}<{method}>d__##.MoveNext() in
        private static readonly Regex ASYNC_REGEX2 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<\>c\.\<\<(?<method>\w+)\>b__[0-9|_]+\>d\.MoveNext\(\)\s+in"); // at {namespace}<>c.<<{method}>b__##_##>d.MoveNext() in
        private static readonly Regex ASYNC_REGEX3 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<\>c__DisplayClass[0-9|_]+\.\<\<(?<method>\w+)\>b__[0-9]+\>d\.MoveNext\(\)"); // at {namespace}<>c__DisplayClass##_##.<<{method}>b__##>d.MoveNext()
        private static readonly Regex ASYNC_REGEX4 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<\>c\.\<(?<method>\w+)\>b__[0-9|_]+\(\)"); // at {namespace}<>c.<{method}>b__##_##() in
        private static readonly Regex ASYNC_REGEX5 = new Regex(@"^\s*at\s+System\.Threading\.Tasks\.Task.*\.InnerInvoke\(\)"); // at System.Threading.Tasks.Task`1.InnerInvoke()

        #region private static readonly string[] IGNORE_START_WITH = ...

        private static readonly string[] IGNORE_START_WITH =
            {
                "at System.Runtime.ExceptionServices.",
                "at System.Runtime.CompilerServices.TaskAwaiter",
                "at System.AppDomain.",
                "at System.Threading.ThreadHelper.ThreadStart_",
                "at System.Threading.ExecutionContext.Run",
                "--- End of stack trace from previous location where exception was thrown ---", // all async method do this
                "at System.Threading.Tasks.Task.Execute()",
                "at System.Threading.Tasks.ContinuationTaskFromTask.InnerInvoke()",
                "at System.Threading.QueueUserWorkItemCallback:",
                "at System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
                "at System.Threading.Tasks.BnayaTaskExtensions.<>c.<ThrowAll>b__10_0(Task c)",
                "at System.Threading.Tasks.Task.InnerInvokeWithArg",
                "at System.Threading.Tasks.Task.<>c__DisplayClass",
                "at System.Threading.Tasks.Task.ThrowIfExceptional",
                "at System.Threading.Tasks.Task.Wait",
                "at System.Threading.Tasks.Parallel.ForWorker",
                "at System.Threading.Tasks.Task.ExecuteWithThreadLocal",
            };

        #endregion

        private static readonly Regex CHAR_OR_NUMERIC = new Regex(@"[\w|\d]"); // char or digit  
        private readonly ErrorFormattingOption _option;
        private readonly char _replaceWith;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyFormatException" /> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="option">Formatting option.</param>
        /// <param name="replaceWith">The replacement char.</param>
        public LazyFormatException(
            Exception innerException,
            ErrorFormattingOption option = Default,
            char replaceWith = '-') : 
                base(string.Empty, innerException)
        {
            _option = option;
            _replaceWith = replaceWith;
        }

        #endregion // Ctor

        #region GetObjectData

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            InnerException.GetObjectData(info, context);
        }

        #endregion // GetObjectData

        #region Source

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        public override string Source 
        { 
            get => InnerException.Source; 
            set => InnerException.Source = value; 
        }

        #endregion // Source

        #region StackTrace

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public override string StackTrace => InnerException.StackTrace;

        #endregion // StackTrace

        #region HelpLink

        /// <summary>
        /// Gets or sets a link to the help file associated with this exception.
        /// </summary>
        public override string HelpLink 
        {   get => InnerException.HelpLink;
            set => InnerException.HelpLink = value; 
        }

        #endregion // HelpLink

        #region Message

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => InnerException.Message;

        #endregion // Message

        #region GetBaseException

        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Exception"></see> that is the root cause of one or more subsequent exceptions.
        /// </summary>
        /// <returns>
        /// The first exception thrown in a chain of exceptions. If the <see cref="P:System.Exception.InnerException"></see> property of the current exception is a null reference (Nothing in Visual Basic), this property returns the current exception.
        /// </returns>
        public override Exception GetBaseException() => InnerException.GetBaseException();

        #endregion // GetBaseException

        #region Data

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        public override IDictionary Data => InnerException.Data;

        #endregion // Data

        #region Equals

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)=>  InnerException.Equals(obj);

        #endregion // Equals

        #region GetHashCode

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => InnerException.GetHashCode();

        #endregion // GetHashCode

        #region ToString

        private string? _toStringCache = null;
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_toStringCache != null)
                return _toStringCache;
            _toStringCache = Format(InnerException, _option, _replaceWith);
            return _toStringCache;
        }

        #endregion // ToString

        #region Format / FormatWithLineNumber

#pragma warning disable MS002 // Cyclomatic Complexity does not follow metric rules.
        /// <summary>
        /// Simplify the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="option">Formatting option.</param>
        /// <param name="replaceWith">The replacement char.</param>
        /// <returns>Formatted exception details</returns>
        private static string Format(
                Exception exception,
                ErrorFormattingOption option = Default,
                char replaceWith = '-')
        {
            if (exception == null)
                return string.Empty;
            try
            {
                var builder = new StringBuilder();
                builder.AppendLine("Root cause:");
                var aggregate = exception as AggregateException;
                if (aggregate == null)
                {
                    var root = exception?.GetBaseException();
                    var rootMessage = root?.Message;
                    builder.AppendLine($"\t{rootMessage}\r\n");
                }
                else
                {
                    foreach (var ex in aggregate.Flatten().InnerExceptions)
                    {
                        var root = ex?.GetBaseException();
                        var rootMessage = root?.Message;
                        builder.AppendLine($"\t{rootMessage}\r\n");
                    }
                }

                builder.AppendLine("Formatted Stacks");
                bool includeLineNumber = (option & IncludeLineNumber) != None;

                if (exception == null)
                    return string.Empty;
                List<string> keep = FormaStack(exception, includeLineNumber);
                string? prev = null;
                int lastCount = 0;
                for (int i = 0; i < keep.Count; i++)
                {
                    string candidate = keep[i];
                    string origin = candidate;
                    if ((option & FormatDuplication) != None)
                    {
                        if (origin.StartsWith(THROW_PREFIX, StringComparison.Ordinal))
                            prev = null;
                        else
                        {
                            int count = 0;
                            if (prev != null)
                                (candidate, count) = HideDuplicatePaths(prev, candidate, replaceWith);
                            prev = origin;
                            if (lastCount > count)
                            {
                                candidate = origin; // when similarity decreased, it write the full information
                                lastCount = 0;
                            }
                            else
                                lastCount = count;
                        }
                    }
                    builder.Append(candidate);
                }

                if ((option & IncludeFullUnformattedDetails) != None)
                {
                    builder.AppendLine("====================== FULL INFORMATION ============================");
                    builder.AppendLine(exception.ToString());
                    builder.AppendLine("====================================================================");
                }
                return builder.ToString();
            }
            catch
            {
                return exception.ToString();
            }
        }
#pragma warning restore MS002 // Cyclomatic Complexity does not follow metric rules.

        #endregion // Format / FormatWithLineNumber

        #region FormaStack

        /// <summary>
        /// Format the call stack
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="withLineNumber">if set to <c>true</c> [with line number].</param>
        /// <returns></returns>
        private static List<string> FormaStack(
            Exception exception,
            bool withLineNumber)
        {
            var aggregate = exception as AggregateException;
            if (aggregate != null)
            {
                var stackDetails = new List<string>();
                var exceptions = aggregate.Flatten().InnerExceptions;
                if (exceptions.Count != 1)
                {
                    int count = 1;
                    foreach (var ex in exceptions)
                    {
                        List<string> tmp = ReBuildStack(ex);
                        stackDetails.Add($"\r\n ~ {count++} ~> ({ex?.GetType()?.Name}) Reason = {ex?.GetBaseException()?.Message}\r\n");
                        stackDetails.AddRange(tmp);
                    }
                    stackDetails.Add(Environment.NewLine);
                    List<string> origin = ReBuildStack(exception);
                    int startTaskIndex = origin.IndexOf(START_TASK_TAG);
                    if (startTaskIndex == -1)
                    {
                        startTaskIndex = 0;
                        stackDetails.Add(START_ASYNC_TAG);
                    }
                    stackDetails.AddRange(origin.Skip(startTaskIndex));
                    return stackDetails;
                }
                exception = exceptions[0]; // single aggregate exception can unwrapped
            }

            var stack = ReBuildStack(exception, string.Empty, withLineNumber);
            return stack;
        }

        #endregion // FormaStack

        #region ReBuildStack

#pragma warning disable MS002 // Cyclomatic Complexity does not follow metric rules.
        /// <summary>
        /// Re-build stack.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="withLineNumber">if set to <c>true</c> [with line number].</param>
        /// <returns></returns>
        private static List<string> ReBuildStack(
            Exception exception,
            string indent = "",
            bool withLineNumber = false)
        {
            var stackDetails = new List<string>();
            while (exception != null)
            {
                var mtd = exception.TargetSite as MethodInfo;
                if (mtd == null)
                {
                    #region Add Full Exception Details

                    stackDetails.Add($"\r\n{indent}-----------------------------\r\n");
                    stackDetails.Add($"{indent}{exception}");
                    stackDetails.Add($"\r\n{indent}-----------------------------\r\n");

                    #endregion // Add Full Exception Details
                    break;
                }

                var tmp = new List<string>();

                #region tmp.Add("# Throw (exception)")

                if (!(exception is AggregateException))
                {
                    string message = string.Empty;
                    using (var reader = new StringReader(exception.Message))
                    {
                        message = reader.ReadLine();
                        if (message.Length > MAX_LEN_OF_INNER_SNAP_LINE)
                            message = message.Substring(0, MAX_LEN_OF_INNER_SNAP_LINE) + " ...";
                    }
                    tmp.Add($"{indent}{THROW_PREFIX} ({exception?.GetType()?.Name}): {message}\r\n");
                }

                #endregion // tmp.Add("# Throw (exception)")

                if (exception == null) continue;
                using (var r = new StringReader(exception.StackTrace))
                {
                    while (true)
                    {
                        string line = r.ReadLine();
                        if (line == null)
                            break;
                        line = line.Trim();

                        #region Validation

                        if (IGNORE_START_WITH.Any(ignore => line.StartsWith(ignore, StringComparison.Ordinal)))
                            continue;

                        #endregion // Validation

                        #region Case: ThreadStart

                        if (line.StartsWith("at System.Threading.ThreadHelper.ThreadStart()", StringComparison.Ordinal))
                        {
                            tmp.Add($"{indent} ~ Start Thread ~>\r\n");
                            continue;
                        }

                        #endregion // Case: ThreadStart

                        #region Clean up MoveNext and other async patterns

                        var m = ASYNC_REGEX1.Match(line);
                        if (m?.Success ?? false)
                        {
                            string at = string.Empty;
                            if (withLineNumber)
                            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                string file = m.Groups?["file"]?.Value;
                                if (file != null)
                                    file = Path.GetFileName(file);
                                string loc = m.Groups?["loc"]?.Value;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                                at = $" at {file} {loc}";
                            }
                            string data = $"{indent}\t{m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?){at} ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX2.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"{indent}\tanonymous: {m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX3.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"{indent}\t{m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX4.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"{indent}\tanonymous: {m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX5.Match(line);
                        if (m?.Success ?? false)
                        {
                            tmp.Add($"{indent}\t{START_TASK_TAG}\r\n");
                            continue;
                        }
                        m = SYNC_REGEX.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = m.Groups?[1]?.Value ?? "Missing";
                            tmp.Add($"{indent}\t{data} ->\r\n");
                            continue;
                        }
                        tmp.Add($"{indent}\t{line}\r\n");

                        #endregion // Clean up MoveNext and other async patterns
                    }
                }

                #region Remove Duplicate Stack

                if (!withLineNumber && tmp.Count != 0)
                {
                    var fst = stackDetails.Skip(1).FirstOrDefault();
                    var lst = tmp[tmp.Count - 1];

                    if (fst != null && fst == lst)
                        tmp.Remove(fst);
                }

                #endregion // Remove Duplicate Stack

                stackDetails.InsertRange(0, tmp);
                exception = exception.InnerException;
            }

            return stackDetails;
        }
#pragma warning restore MS002 // Cyclomatic Complexity does not follow metric rules.

        #endregion // ReBuildStack

        #region HideDuplicatePaths

        /// <summary>
        /// Dashes the compare.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dest">The destination.</param>
        /// <param name="replaceWith">The replacement char.</param>
        /// <returns>the new string and the count of parts which was replaced</returns>
        private static (string Candidate, int Count) HideDuplicatePaths(string src, string dest, char replaceWith = '-')
        {
            var pathBuilder = new StringBuilder();
            string[] dests = dest.Split('.');
            string replacement = replaceWith.ToString();
            int count = 0;
            foreach (var d in dests)
            {
                string search = d + ".";
                int len = pathBuilder.Length;
                int index = src.IndexOf(search, len, StringComparison.Ordinal);
                if (index != len)
                    break;
                string part = CHAR_OR_NUMERIC.Replace(search, replacement);
                pathBuilder.Append(part);
                count++;
            }
            if (pathBuilder.Length == 0)
                return (dest, 0);
            return ($"{pathBuilder}{dest.Substring(pathBuilder.Length)}", count);
        }

        #endregion // HideDuplicatePaths

        #region Casting [operator overloads]

        /// <summary>
        /// Performs an implicit conversion from <see cref="LazyFormatException"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(LazyFormatException instance)
        {
            return instance.ToString();
        }

        #endregion // Casting [operator overloads]
    }
}

