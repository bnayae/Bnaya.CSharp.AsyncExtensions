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
    public static class BnayaErrorHandlinglExtensions
    {
        private const int MAX_LEN_OF_INNER_SNAP_LINE = 50;
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
            };

        #region Format

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
                List<string> keep = FormarRec(exception);
                for (int i = 0; i < keep.Count; i++)
                {
                    // TODO: Cleanup duplication (namespace / class name)
                    // TODO: try to capture the parameters
                    string prev = string.Empty;
                    if (i > 0)
                        prev = keep[i - 1];
                    builder.Append(keep[i]);
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

        #region HandleExceptionFlow

        /// <summary>
        /// Recursive formatting
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Build it in reverse format</returns>
        private static List<string> FormarRec(Exception exception)
        {
            var stackDetails = new List<string>();
            var aggregate = exception as AggregateException;
            if (aggregate != null)
            {
                var exceptions = aggregate.Flatten().InnerExceptions;
                if (exceptions.Count != 1)
                {
                    int count = 1;
                    foreach (var ex in exceptions)
                    {
                        List<string> tmp = FormarRec(ex);
                        stackDetails.Add($"\r\n ~ {count++} ~> ({ex?.GetType()?.Name}) Reason = {ex?.GetBaseException()?.Message}\r\n");
                        stackDetails.AddRange(tmp);
                    }
                    return stackDetails;
                }
                exception = exceptions[0];
            }

            while (exception != null)
            {
                var mtd = exception.TargetSite as MethodInfo;
                if (mtd == null)
                {
                    #region Add Full Exception Details

                    stackDetails.Add("\r\n-----------------------------\r\n");
                    stackDetails.Add(exception.ToString());
                    stackDetails.Add("\r\n-----------------------------\r\n");

                    #endregion // Add Full Exception Details
                    break;
                }

                var tmp = new List<string>();

                #region tmp.Add("# Throw (exception)")

                string message = string.Empty;
                using (var reader = new StringReader(exception.Message))
                {
                    message = reader.ReadLine();
                    if (message.Length > MAX_LEN_OF_INNER_SNAP_LINE)
                        message = message.Substring(0, MAX_LEN_OF_INNER_SNAP_LINE) + " ...";
                }
                tmp.Add($"  # Throw ({exception?.GetType()?.Name}): {message}\r\n");

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

                        if (line.StartsWith("at System.Threading.ThreadHelper.ThreadStart()"))
                        {
                            tmp.Add("\tStart Thread:");
                            continue;
                        }
                        var m = ASYNC_REGEX1.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"\t{m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX2.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"\tanonymous: {m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX3.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"\t{m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX4.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = $"\tanonymous: {m.Groups?["namespace"]?.Value ?? "Missing"}{m.Groups?["method"]?.Value ?? "Missing"}(?) ->\r\n";
                            tmp.Add(data);
                            continue;
                        }
                        m = ASYNC_REGEX5.Match(line);
                        if (m?.Success ?? false)
                        {
                            tmp.Add("\tStart Task: ");
                            continue;
                        }
                        m = SYNC_REGEX.Match(line);
                        if (m?.Success ?? false)
                        {
                            string data = m.Groups?[1]?.Value ?? "Missing";
                            tmp.Add($"\t{data} ->\r\n");
                            continue;
                        }
                        tmp.Add($"\t{line}\r\n");
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

        #endregion // HandleExceptionFlow
    }
}

