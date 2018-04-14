// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    /// <summary>Provides an awaiter for a <see cref="TaskLike{TResult}"/>.</summary>
    public struct TaskLikeAwaiter<TResult> : ICriticalNotifyCompletion
    {
        #region Fields

        /// <summary>
        /// The value being awaited.
        /// </summary>
        private readonly TaskLike<TResult> _value;

        #endregion // Fields

        #region Ctor

        /// <summary>
        /// Initializes the awaiter.
        /// </summary>
        /// <param name="value">The value to be awaited.</param>
        internal TaskLikeAwaiter(TaskLike<TResult> value)
        {
            _value = value;
        }

        #endregion // Ctor

        #region IsCompleted

        /// <summary>
        /// Gets whether the <see cref="TaskLike{TResult}" /> has completed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleted => _value.IsCompleted;

        #endregion // IsCompleted

        #region GetResult

        /// <summary>
        /// Gets the result of the TaskLike.
        /// </summary>
        /// <returns></returns>
        public TResult GetResult()
        {
            return _value._task == null ?
                _value._result :
                _value._task.GetAwaiter().GetResult();
        }

        #endregion // GetResult

        #region OnCompleted

        /// <summary>
        /// Schedules the continuation action for this TaskLike.
        /// </summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        public void OnCompleted(Action continuation)
        {
            _value.AsTask().ConfigureAwait(continueOnCapturedContext: true).GetAwaiter().OnCompleted(continuation);
        }

        #endregion // OnCompleted

        #region UnsafeOnCompleted

        /// <summary>
        /// Schedules the continuation action for this TaskLike.
        /// </summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        public void UnsafeOnCompleted(Action continuation)
        {
            _value.AsTask().ConfigureAwait(continueOnCapturedContext: true).GetAwaiter().UnsafeOnCompleted(continuation);
        }

        #endregion // UnsafeOnCompleted
    }
}