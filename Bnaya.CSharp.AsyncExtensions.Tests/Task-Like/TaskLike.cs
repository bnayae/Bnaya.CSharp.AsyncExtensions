﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable RCS1165 // Unconstrained type parameter checked for null.

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides a light type (which extending  Value Task) that wraps a <see cref="Task{TResult}"/> and a <typeparamref name="TResult"/>,
    /// only one of which is used.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>
    /// <para>
    /// Methods may return an instance of this value type when it's likely that the result of their
    /// operations will be available synchronously and when the method is expected to be invoked so
    /// frequently that the cost of allocating a new <see cref="Task{TResult}"/> for each call will
    /// be prohibitive.
    /// </para>
    /// <para>
    /// There are tradeoffs to using a <see cref="TaskLike{TResult}"/> instead of a <see cref="Task{TResult}"/>.
    /// For example, while a <see cref="TaskLike{TResult}"/> can help avoid an allocation in the case where the 
    /// successful result is available synchronously, it also contains two fields whereas a <see cref="Task{TResult}"/>
    /// as a reference type is a single field.  This means that a method call ends up returning two fields worth of
    /// data instead of one, which is more data to copy.  It also means that if a method that returns one of these
    /// is awaited within an async method, the state machine for that async method will be larger due to needing
    /// to store the struct that's two fields instead of a single reference.
    /// </para>
    /// <para>
    /// Further, for uses other than consuming the result of an asynchronous operation via await, 
    /// <see cref="TaskLike{TResult}"/> can lead to a more convoluted programming model, which can in turn actually 
    /// lead to more allocations.  For example, consider a method that could return either a <see cref="Task{TResult}"/> 
    /// with a cached task as a common result or a <see cref="TaskLike{TResult}"/>.  If the consumer of the result 
    /// wants to use it as a <see cref="Task{TResult}"/>, such as to use with in methods like Task.WhenAll and Task.WhenAny, 
    /// the <see cref="TaskLike{TResult}"/> would first need to be converted into a <see cref="Task{TResult}"/> using 
    /// <see cref="TaskLike{TResult}.AsTask"/>, which leads to an allocation that would have been avoided if a cached 
    /// <see cref="Task{TResult}"/> had been used in the first place.
    /// </para>
    /// <para>
    /// As such, the default choice for any asynchronous method should be to return a <see cref="Task"/> or 
    /// <see cref="Task{TResult}"/>. Only if performance analysis proves it worthwhile should a <see cref="TaskLike{TResult}"/> 
    /// be used instead of <see cref="Task{TResult}"/>.  There is no non-generic version of <see cref="TaskLike{TResult}"/> 
    /// as the Task.CompletedTask property may be used to hand back a successfully completed singleton in the case where
    /// a <see cref="Task"/>-returning method completes synchronously and successfully.
    /// </para>
    /// </remarks>
    [AsyncMethodBuilder(typeof(TaskLikeMethodBuilder<>))]
    [StructLayout(LayoutKind.Auto)]
    public struct TaskLike<TResult> : IEquatable<TaskLike<TResult>>
    {
        #region Fields

        /// <summary>
        /// The task to be used if the operation completed asynchronously or if it completed synchronously but non-successfully.
        /// </summary>
        internal readonly Task<TResult> _task;
        /// <summary>
        /// The result to be used if the operation completed successfully synchronously.
        /// </summary>
        internal readonly TResult _result;

        #endregion // Fields

        #region Ctor

        /// <summary>
        /// Initialize the <see cref="TaskLike{TResult}" /> with the result of the successful operation.
        /// </summary>
        /// <param name="result">The result.</param>
        public TaskLike(TResult result)
        {
            _task = null;
            _result = result;
        }

        /// <summary>
        /// Initialize the <see cref="TaskLike{TResult}" /> with a <see cref="Task{TResult}" /> that represents the operation.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <exception cref="ArgumentNullException">task</exception>
        public TaskLike(Task<TResult> task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _result = default;
        }

        #endregion // Ctor

        #region IsCompleted

        /// <summary>
        /// Gets whether the <see cref="TaskLike{TResult}" /> represents a completed operation.
        /// </summary>
        public bool IsCompleted => _task == null || _task.IsCompleted;

        #endregion // IsCompleted

        #region IsCompletedSuccessfully

        /// <summary>
        /// Gets whether the <see cref="TaskLike{TResult}" /> represents a successfully completed operation.
        /// </summary>
        public bool IsCompletedSuccessfully => _task == null || _task.Status == TaskStatus.RanToCompletion;

        #endregion // IsCompletedSuccessfully

        #region IsFaulted

        /// <summary>
        /// Gets whether the <see cref="TaskLike{TResult}" /> represents a failed operation.
        /// </summary>
        public bool IsFaulted => _task != null && _task.IsFaulted;

        #endregion // IsFaulted

        #region IsCanceled

        /// <summary>
        /// Gets whether the <see cref="TaskLike{TResult}" /> represents a canceled operation.
        /// </summary>
        public bool IsCanceled => _task != null && _task.IsCanceled;

        #endregion // IsCanceled

        #region Result

        /// <summary>
        /// Gets the result.
        /// </summary>
        public TResult Result => _task == null ? _result : _task.GetAwaiter().GetResult();

        #endregion // Result

        #region AsTask

        /// <summary>
        /// Gets a <see cref="Task{TResult}"/> object to represent this TaskLike.  It will
        /// either return the wrapped task object if one exists, or it'll manufacture a new
        /// task object to represent the result.
        /// </summary>
        public Task<TResult> AsTask()
        {
            // Return the task if we were constructed from one, otherwise manufacture one.  We don't
            // cache the generated task into _task as it would end up changing both equality comparison
            // and the hash code we generate in GetHashCode.
            return _task ?? Task.FromResult(_result);
        }

        #endregion // AsTask

        #region GetAwaiter

        /// <summary>
        /// Gets an awaiter for this value.
        /// </summary>
        /// <returns></returns>
        public TaskLikeAwaiter<TResult> GetAwaiter()
        {
            return new TaskLikeAwaiter<TResult>(this);
        }

        #endregion // GetAwaiter

        #region ConfigureAwait

        /// <summary>
        /// Configures an awaiter for this value.
        /// </summary>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the captured context; otherwise, false.</param>
        /// <returns></returns>
        public ConfiguredTaskLikeAwaitable<TResult> ConfigureAwait(
                                        bool continueOnCapturedContext)
        {
            return new ConfiguredTaskLikeAwaitable<TResult>(this, continueOnCapturedContext: continueOnCapturedContext);
        }

        #endregion // ConfigureAwait

        #region ToString

        /// <summary>
        /// Gets a string-representation of this <see cref="TaskLike{TResult}" />.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_task != null)
            {
                if(_task.Status == TaskStatus.RanToCompletion)
                    return 
                        _task?.Result?.ToString() ?? string.Empty;
                return string.Empty;
            }
            else
            {
                return _result?.ToString() ?? string.Empty;
            }
        }

        #endregion // ToString

        #region Equality Pattern

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
 #pragma warning disable RECS0098 // Finds redundant null coalescing expressions such as expr ?? expr
           return
                _task != null ? _task.GetHashCode() :
                                _result?.GetHashCode() ?? 0;
#pragma warning restore RECS0098 // Finds redundant null coalescing expressions such as expr ?? expr
        }

        /// <summary>
        /// Returns a value indicating whether this value is equal to a specified <see cref="object" />.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return
                obj is TaskLike<TResult> &&
                Equals((TaskLike<TResult>)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this value is equal to a specified <see cref="TaskLike{TResult}" /> value.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(TaskLike<TResult> other)
        {
            if (_task != null || other._task != null)
                return _task == other._task;
            return EqualityComparer<TResult>.Default.Equals(_result, other._result);
        }

        /// <summary>
        /// Returns a value indicating whether two <see cref="TaskLike{TResult}" /> values are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(TaskLike<TResult> left, TaskLike<TResult> right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns a value indicating whether two <see cref="TaskLike{TResult}"/> values are not equal.</summary>
        public static bool operator !=(TaskLike<TResult> left, TaskLike<TResult> right)
        {
            return !left.Equals(right);
        }

        #endregion // Equality Pattern

        #region Casting [operator overloads]

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Threading.Tasks.TaskLike{TResult}" /> to <see cref="System.Threading.Tasks.Task{TResult}" />.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Task<TResult>(TaskLike<TResult> instance)
        {
            //implicit cast logic
            return instance.AsTask();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Threading.Tasks.TaskLike{TResult}" /> to <see cref="System.Threading.Tasks.ValueTask{TResult}" />.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ValueTask<TResult>(TaskLike<TResult> instance)
        {
            //implicit cast logic
            if (instance._task != null)
                return new ValueTask<TResult>(instance._task);
            return new ValueTask<TResult>(instance._result);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Threading.Tasks.Task{TResult}" /> to <see cref="System.Threading.Tasks.TaskLike{TResult}" />.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator TaskLike<TResult>(Task<TResult> instance)
        {
            //implicit cast logic
            return new TaskLike<TResult>(instance);
        }

        /// <summary>
        /// Performs an implicit conversion from TResult to TaskLike{TResult}".
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator TaskLike<TResult>(TResult instance)
        {
            //implicit cast logic
            return new TaskLike<TResult>(instance);
        }

        #endregion // Casting [operator overloads]

        #region CreateAsyncMethodBuilder [compiler]

        /// <summary>
        /// Creates a method builder for use with an async method.
        /// </summary>
        /// <returns>
        /// The created builder.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)] // intended only for compiler consumption
        public static TaskLikeMethodBuilder<TResult> CreateAsyncMethodBuilder() => 
                        TaskLikeMethodBuilder<TResult>.Create();

        #endregion // CreateAsyncMethodBuilder [compiler]
    }
}
