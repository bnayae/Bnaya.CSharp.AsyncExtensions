using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Bnaya.CSharp.AsyncExtensions.Tests")]

namespace System.Threading.Tasks
{
    /// <summary>
    /// Task Extensions
    /// </summary>
    public static class BnayaErrorHandlinglExtensions
    {
        private const int MAX_LEN_OF_INNER_SNAP_LINE = 50;
        private const string THROW_PREFIX = "  # Throw";
        private const string START_TASK_TAG = " ~ Start Task ~>\r\n";
        private const string START_ASYNC_TAG = " ~ Start ~>\r\n";
        //private static readonly Regex ASYNC_REGEX = new Regex(@"\.(.*)\.<(.*)>d__"); // .{group 0 - any}.<{group 1 = any}>d__
        // "^\s*at = start with 'at ' optional preceding whitespace 
        // (.*\)) = group any until ')'
        private static readonly Regex SYNC_REGEX = new Regex(@"^\s*at (.*\))");

        private static readonly Regex EXCLUDE = new Regex(@"[\W|_]"); // not char, digit  
        private static readonly Regex EXCLUDE_FROM_START = new Regex(@"^[\W|_]"); // not start with char, digit  
        private static readonly Regex ASYNC_REGEX1 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<(?<method>\w+)\>d__[0-9]+.MoveNext\(\)\sin"); // at {nameespace}<{method}>d__##.MoveNext() in
        private static readonly Regex ASYNC_REGEX2 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<\>c\.\<\<(?<method>\w+)\>b__[0-9|_]+\>d\.MoveNext\(\)\s+in"); // at {namespace}<>c.<<{method}>b__##_##>d.MoveNext() in
        private static readonly Regex ASYNC_REGEX3 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<\>c__DisplayClass[0-9|_]+\.\<\<(?<method>\w+)\>b__[0-9]+\>d\.MoveNext\(\)"); // at {namespace}<>c__DisplayClass##_##.<<{method}>b__##>d.MoveNext()
        private static readonly Regex ASYNC_REGEX4 = new Regex(@"^\s*at\s+(?<namespace>[\w|\.]+)\<\>c\.\<(?<method>\w+)\>b__[0-9|_]+\(\)"); // at {namespace}<>c.<{method}>b__##_##() in
        private static readonly Regex ASYNC_REGEX5 = new Regex(@"^\s*at\s+System\.Threading\.Tasks\.Task.*\.InnerInvoke\(\)"); // at System.Threading.Tasks.Task`1.InnerInvoke()
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
            };

        private static readonly Regex CHAR_OR_NUMERIC = new Regex(@"[\w|\d]"); // char or digit  

        #region Format

        #region Overloads

        /// <summary>
        /// Simplify the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="includeFullUnformatedDetails">if set to <c>true</c> [include exception.ToString()].</param>
        /// <returns></returns>
        public static string Format(
                this Exception exception,
                bool includeFullUnformatedDetails = false)
        {
            return Format(exception, ErrorFormattingOption.Default, includeFullUnformatedDetails);
        }

        #endregion // Overloads

        /// <summary>
        /// Simplify the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="option">Formatting option.</param>
        /// <param name="includeFullUnformatedDetails">if set to <c>true</c> [include exception.ToString()].</param>
        /// <param name="replaceWith">The replacement char.</param>
        /// <returns></returns>
        internal static string Format(
                this Exception exception,
                ErrorFormattingOption option,
                bool includeFullUnformatedDetails = false,
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
                List<string> keep = FormaStack(exception);
                string prev = null;
                int lastCount = 0;
                for (int i = 0; i < keep.Count; i++)
                {
                    // TODO: try to capture the parameters
                    string candidate = keep[i];
                    string origin = candidate;
                    if (option == ErrorFormattingOption.FormatDuplication)
                    {
                        if (origin.StartsWith(THROW_PREFIX)) 
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

                if (includeFullUnformatedDetails)
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

        #endregion // Format

        #region FormaStack

        /// <summary>
        /// Format the call stack
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static List<string> FormaStack(Exception exception)
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

            var stack = ReBuildStack(exception);
            return stack;
        }

        #endregion // FormaStack

        #region ReBuildStack

        private static List<string> ReBuildStack(Exception exception, string indent = "")
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

                using (var r = new StringReader(exception.StackTrace))
                {
                    while (true)
                    {
                        string line = r.ReadLine();
                        if (line == null)
                            break;
                        line = line.Trim();

                        #region Validation

                        if (IGNORE_START_WITH.Any(ignore => line.StartsWith(ignore)))
                            continue;

                        #endregion // Validation

                        #region Case: ThreadStart

                        if (line.StartsWith("at System.Threading.ThreadHelper.ThreadStart()"))
                        {
                            tmp.Add($"{indent} ~ Start Thread ~>\r\n");
                            continue;
                        }

                        #endregion // Case: ThreadStart

                        #region Clean up MoveNext and other async patterns

                        var m = ASYNC_REGEX1.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"{indent}\t{m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
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
                            tmp.Add($"{indent}{START_TASK_TAG}");
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

                if (tmp.Count != 0)
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

        #endregion // ReBuildStack

        // TODO: keep \r\n\t
        #region HideDuplicatePaths

        /// <summary>
        /// Dashes the compare.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dest">The destination.</param>
        /// <param name="replaceWith">The replacement char.</param>
        /// <returns>the new string and the count of parts which was replaced</returns>
        public static (string Candidate, int Count) HideDuplicatePaths(string src, string dest, char replaceWith = '-')
        {
            var pathBuilder = new StringBuilder();
            string[] dests = dest.Split('.');
            string replacement = replaceWith.ToString();
            int count = 0;
            foreach (var d in dests)
            {
                string search = d + ".";
                int len = pathBuilder.Length;
                int index = src.IndexOf(search, len);
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

    }
}

