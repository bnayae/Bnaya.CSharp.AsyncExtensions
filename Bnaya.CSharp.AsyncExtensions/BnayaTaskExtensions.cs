﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Task Extensions
    /// </summary>
    public static class BnayaTaskExtensions
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(15);

        #region Try/AcquireAsync

        /// <summary>
        /// Try to acquire async lock,
        /// when failed the LockScope.Acquired will equals false
        /// </summary>
        /// <param name="gate">The gate.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>lock disposal and acquired indication</returns>
        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Task<LockScope> TryAcquireAsync(this SemaphoreSlim gate, TimeSpan timeout)
        {
            var locker = new AsyncLock(gate, timeout);
            return locker.TryAcquireAsync();
        }

        /// <summary>
        /// Try to acquire async lock,
        /// when failed the LockScope.Acquired will equals false
        /// </summary>
        /// <param name="gate">The gate.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>lock disposal</returns>
        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Task<IDisposable> AcquireAsync(this SemaphoreSlim gate, TimeSpan timeout)
        {
            var locker = new AsyncLock(gate, timeout);
            return locker.AcquireAsync();
        }

        #endregion // Try/AcquireAsync

        #region WithTimeout

        /// <summary>
        /// Creates task with timeout.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Operations was timeout after [{timeout.TotalMinutes:N3}]</exception>
        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static async Task<T> WithTimeout<T>(
            this Task<T> task,
            TimeSpan timeout = default(TimeSpan))
        {
            //return task;
            await WithTimeout((Task)task, timeout).ConfigureAwait(false);

            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Creates task with timeout.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Operations was timeout after [{timeout.TotalMinutes:N3}]</exception>
        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static async Task WithTimeout(
            this Task task,
            TimeSpan timeout = default(TimeSpan))
        {
            //return task;
            if (timeout == default(TimeSpan))
                timeout = DefaultTimeout;

            Task check = await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);
            if (check != task)
            {
                throw new TimeoutException($"Operations was timeout after [{timeout.TotalMinutes:N3}] minutes");
            }
            else
                await task.ConfigureAwait(false); // throw if faulted
        }

        #endregion // WithTimeout

        #region IsTimeoutAsync

        /// <summary>
        /// Check for completion of task.
        /// Ideal for checking for expected execution duration without exception.
        /// Can be use to produce warning in case of potential deadlocks.
        /// </summary>
        /// <param name="initTask">The initialize task.</param>
        /// <param name="duration">The duration.</param>
        /// <returns>
        /// true: when timeout.
        /// false: when complete before the timeout.
        /// </returns>
        [DebuggerNonUserCode]
        [Obsolete("Bug fix on version 1.0.17: the return value on previous version returns true when not timeout (should return true when timeout)", false)]
        public static async Task<bool> IsTimeoutAsync(
            this Task initTask,
            TimeSpan duration)
        {
            Task delay = Task.Delay(duration);
            Task deadlockDetection = await Task.WhenAny(initTask, delay).ConfigureAwait(false);
            if (deadlockDetection == delay)
            {
                return true;
            }
            else
            {
                await initTask.ConfigureAwait(false);
                return false;
            }
        }

        #endregion // IsTimeoutAsync

        #region CancelSafe

        /// <summary>
        /// Cancels the safe.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public static bool CancelSafe(
            this CancellationTokenSource instance)
        {
            return CancelSafe(instance, out Exception? ex);
        }

        /// <summary>
        /// Cancels the safe.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="exc">The exception (on fault cancellation).</param>
        /// <returns></returns>
        public static bool CancelSafe(
            this CancellationTokenSource instance, out Exception? exc)
        {
            exc = null;
            try
            {
                instance.Cancel();
            }
            #region Exception Handling

            catch (Exception ex)
            {
                exc = ex;
                return false;
            }

            #endregion // Exception Handling
            return true;
        }

        #endregion // WithCancellation

        #region [Deprecated] RegisterWeak

        /// <summary>
        /// Weak registration for cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        [Obsolete("CancellationToken.Register become weak, no need for this method, will be deleted next version", false)]
        public static CancellationTokenRegistration RegisterWeak(
            this CancellationToken cancellation,
            Action action)
        {
            var weak = new WeakReference<Action>(action);
            Action tmp = () =>
            {
                if (weak.TryGetTarget(out Action act))
                    act();
            };
            return cancellation.Register(tmp);
        }

        /// <summary>
        /// Weak registration for cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <param name="action">The action.</param>
        /// <param name="state">The state.</param>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        [Obsolete("CancellationToken.Register become weak, no need for this method, will be deleted next version", false)]
        public static CancellationTokenRegistration RegisterWeak(
            this CancellationToken cancellation,
            Action<object> action,
            object state)
        {
            var weak = new WeakReference<Action<object>>(action);
            Action<object> tmp = (state_) =>
            {
                if (weak.TryGetTarget(out Action<object> act))
                    act(state_);
            };
            return cancellation.Register(tmp, state);
        }

        #endregion // [Deprecated] RegisterWeak

        #region ThrowAll

        /// <summary>
        /// Throws all (when catching exception withing async / await
        /// and there is potential for multiple exception to be thrown.
        /// async / await will propagate single exception.
        /// in order to catch all the exception use this extension).
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Task ThrowAll(this Task t)
        {
            var result = t.ContinueWith(c =>
            {
                if (c.Exception == null)
                    return;
                throw new AggregateException(c.Exception.Flatten().InnerExceptions);
            });
            return result;
        }

        /// <summary>
        /// Throws all (when catching exception withing async / await
        /// and there is potential for multiple exception to be thrown.
        /// async / await will propagate single exception.
        /// in order to catch all the exception use this extension).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Task<T> ThrowAll<T>(this Task<T> t)
        {
            var result = t.ContinueWith(c =>
            {
                if (c.Exception == null)
                    return c.Result;
                throw new AggregateException(c.Exception.Flatten().InnerExceptions);
            });
            return result;
        }

        #endregion // ThrowAll

        #region ToValueTask

        /// <summary>
        /// Convert any object to value task.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ValueTask<T> ToValueTask<T>(this T value) =>
            new ValueTask<T>(value);

        #endregion // ToValueTask

        #region When

        /// <summary>
        /// Return task which complete pass condition and return first
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks">The tasks.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="cancellation">The cancellation id handle
        /// which enable will be set to cancel
        /// when completed tasks (which pass the condition)
        /// reach the threshold.
        /// It's enable the original tasks to listen on its cancellation token.</param>
        /// <returns>
        /// The result of the task which pass the condition (in case that it didn't succeed, return default).
        /// Succeed: indicate whether completed task
        /// (which pass the condition) reach the threshold.
        /// </returns>
        public static async Task<(T? Result, bool Succeed, Task<T[]> All)> When<T>(
            this IEnumerable<Task<T>> tasks,
            Func<T, bool> condition,
            CancellationTokenSource? cancellation = null)
        {
            (T[] items, bool succeed, Task<T[]> all) = await WhenN(tasks, 1, condition, cancellation);
            if(succeed)
                return (items.FirstOrDefault(), succeed, all);
            return (default(T), succeed, all);
        }

        #endregion // When

        #region WhenN

        #region Overloads

        /// <summary>
        /// Return task which complete when completed tasks cross the threshold
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="cancellation">The cancellation id handle
        /// which enable will be set to cancel
        /// when completed tasks (which pass the condition)
        /// reach the threshold.
        /// It's enable the original tasks to listen on its cancellation token.</param>
        /// <returns>
        /// Outer Task represent the completion of N Tasks.
        /// Inner Task represent the completion of all Tasks
        /// </returns>
        public static Task<Task> WhenN(
            this IEnumerable<Task> tasks,
            int threshold,
            CancellationTokenSource? cancellation = null)
        {
            return WhenN(tasks.ToArray(), threshold, cancellation);
        }

        /// <summary>
        /// Return task which complete when completed tasks (that pass condition) cross the threshold
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks">The tasks.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="cancellation">The cancellation id handle
        /// which enable will be set to cancel
        /// when completed tasks (which pass the condition)
        /// reach the threshold.
        /// It's enable the original tasks to listen on its cancellation token.</param>
        /// <returns>
        /// Results: ordered by completions,
        /// first completed task's result is the first item in the array.
        /// Succeed: indicate whether completed tasks
        /// (which pass the condition) reach the threshold.
        /// All: indication of all completed tasks.
        /// </returns>
        public static Task<(T[] Results, bool Succeed, Task<T[]> All)> WhenN<T>(
            this IEnumerable<Task<T>> tasks,
            int threshold,
            Func<T, bool>? condition = null,
            CancellationTokenSource? cancellation = null)
        {
            return WhenN(tasks.ToArray(), threshold, condition, cancellation);
        }

        #endregion // Overloads

        /// <summary>
        /// Return task which complete when completed tasks (that pass condition) cross the threshold
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks">The tasks.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="cancellation">The cancellation id handle
        /// which enable will be set to cancel
        /// when completed tasks (which pass the condition)
        /// reach the threshold.
        /// It's enable the original tasks to listen on its cancellation token.</param>
        /// <returns>
        /// Results: ordered by completions,
        /// first completed task's result is the first item in the array.
        /// Succeed: indicate whether completed tasks
        /// (which pass the condition) reach the threshold.
        /// All: indication of all completed tasks.
        /// </returns>
        public static async Task<(T[] Results, bool Succeed, Task<T[]> All)> WhenN<T>(
            this Task<T>[] tasks,
            int threshold,
            Func<T, bool>? condition = null,
            CancellationTokenSource? cancellation = null)
        {
            // use for signal completion
            var completionEvent = new TaskCompletionSource<object?>();
            var queue = new ConcurrentQueue<T>();

            Task<T[]> allTasks = Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                Task fireForget = EvalueateSingleTask(task);
            }
            Task anyTask = await Task.WhenAny(
                                    completionEvent.Task, // Succeed
                                    allTasks) // Passing tasks below threshold
                                    .ConfigureAwait(false);

            bool succeed = anyTask == completionEvent.Task;
            T[] response = queue.ToArray();
            return (response, succeed, allTasks);

            #region EvalueateSingleTask (local method)

            async Task EvalueateSingleTask(Task<T> task)
            {
                if (cancellation?.IsCancellationRequested ?? false)
                    return;
                T result = await task;
                if (condition?.Invoke(result) ?? true)
                {
                    queue.Enqueue(result);
                    if (queue.Count >= threshold)
                    {
                        cancellation?.CancelSafe();
                        completionEvent.TrySetResult(null);
                    }
                }
            }

            #endregion // EvalueateSingleTask (local method)
        }

        /// <summary>
        /// Return task which complete when completed tasks cross the threshold
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="cancellation">The cancellation id handle
        /// which enable will be set to cancel
        /// when completed tasks reach the threshold.
        /// It's enable the original tasks to listen on its cancellation token.</param>
        /// <returns>
        /// Outer Task represent the completion of N Tasks.
        /// Inner Task represent the completion of all Tasks
        /// </returns>
        public static async Task<Task> WhenN(
            this Task[] tasks,
            int threshold,
            CancellationTokenSource? cancellation = null)
        {
            // use for signal completion
            var completionEvent = new TaskCompletionSource<object?>();
            int completeCount = 0;

            Task allTasks = Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                Task fireForget = EvalueateSingleTask(task);
            }
            Task anyTask = await Task.WhenAny(
                                    completionEvent.Task, // Succeed
                                    allTasks) // Passing tasks below threshold
                                    .ConfigureAwait(false);

            return allTasks;

            #region EvalueateSingleTask (local method)

            async Task EvalueateSingleTask(Task task)
            {
                if (cancellation?.IsCancellationRequested ?? false)
                    return;
                await task;
                int count = Interlocked.Increment(ref completeCount);
                if (count >= threshold)
                {
                    cancellation?.CancelSafe();
                    completionEvent.TrySetResult(null);
                }
            }

            #endregion // EvalueateSingleTask (local method)
        }


        #endregion // WhenN
    }
}