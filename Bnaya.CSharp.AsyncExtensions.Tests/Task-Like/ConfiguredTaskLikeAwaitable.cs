// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    /// <summary>Provides an awaitable type that enables configured awaits on a <see cref="TaskLike{TResult}"/>.</summary>
    /// <typeparam name="TResult">The type of the result produced.</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public struct ConfiguredTaskLikeAwaitable<TResult>
    {
        #region Fields

        /// <summary>
        /// The wrapped <see cref="TaskLike{TResult}" />.
        /// </summary>
        private readonly TaskLike<TResult> _value;
        /// <summary>
        /// True to attempt to marshal the continuation 
        /// back to the original context captured; otherwise, false.
        /// </summary>
        private readonly bool _continueOnCapturedContext;

        #endregion // Fields

        #region Ctor

        /// <summary>
        /// Initializes the awaitable.
        /// </summary>
        /// <param name="value">The wrapped <see cref="TaskLike{TResult}" />.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original synchronization context captured; otherwise, false.</param>
        internal ConfiguredTaskLikeAwaitable(
            TaskLike<TResult> value, 
            bool continueOnCapturedContext)
        {
            _value = value;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        #endregion // Ctor

        #region GetAwaiter

        /// <summary>
        /// Returns an awaiter for this <see cref="ConfiguredTaskLikeAwaitable{TResult}" /> instance.
        /// </summary>
        /// <returns></returns>
        public ConfiguredTaskLikeAwaiter GetAwaiter()
        {
            return new ConfiguredTaskLikeAwaiter(_value, _continueOnCapturedContext);
        }

        #endregion // GetAwaiter

        #region ConfiguredTaskLikeAwaiter [nested]

        /// <summary>Provides an awaiter for a <see cref="ConfiguredTaskLikeAwaitable{TResult}"/>.</summary>
        [StructLayout(LayoutKind.Auto)]
        public struct ConfiguredTaskLikeAwaiter : ICriticalNotifyCompletion
        {
            #region Fields

            /// <summary>
            /// The value being awaited.
            /// </summary>
            private readonly TaskLike<TResult> _value;
            /// <summary>The value to pass to ConfigureAwait.</summary>
            private readonly bool _continueOnCapturedContext;

            #endregion // Fields

            #region Ctor

            /// <summary>Initializes the awaiter.</summary>
            /// <param name="value">The value to be awaited.</param>
            /// <param name="continueOnCapturedContext">The value to pass to ConfigureAwait.</param>
            internal ConfiguredTaskLikeAwaiter(
                TaskLike<TResult> value,
                bool continueOnCapturedContext)
            {
                _value = value;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            #endregion // Ctor

            #region IsCompleted

            /// <summary>
            /// Gets whether the <see cref="ConfiguredTaskLikeAwaitable{TResult}" />
            /// has completed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
            /// </value>
            public bool IsCompleted { get { return _value.IsCompleted; } }

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
            /// Schedules the continuation action for 
            /// the <see cref="ConfiguredTaskLikeAwaitable{TResult}" />.
            /// </summary>
            /// <param name="continuation">
            /// The action to invoke when the operation completes.
            /// </param>
            public void OnCompleted(Action continuation)
            {
                _value.AsTask() // use the underline task to do the actual job
                    .ConfigureAwait(_continueOnCapturedContext)
                    .GetAwaiter()
                    .OnCompleted(continuation);
            }

            #endregion // OnCompleted

            #region UnsafeOnCompleted

            /// <summary>Schedules the continuation action for the <see cref="ConfiguredTaskLikeAwaitable{TResult}"/>.</summary>
            public void UnsafeOnCompleted(Action continuation)
            {
                _value.AsTask() // use the underline task to do the actual job
                    .ConfigureAwait(_continueOnCapturedContext)
                    .GetAwaiter()
                    .UnsafeOnCompleted(continuation);
            }

            #endregion // UnsafeOnCompleted
        }

        #endregion // ConfiguredTaskLikeAwaiter [nested]
    }
}