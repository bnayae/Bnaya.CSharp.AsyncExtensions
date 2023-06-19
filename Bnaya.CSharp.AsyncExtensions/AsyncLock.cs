
using System.Diagnostics;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represent Semaphore scoping factory.
    /// The semaphore scope will enable the usage of semaphore easy like lock.
    /// </summary>
    public sealed class AsyncLock: IDisposable
    {
        private readonly SemaphoreSlim _gate;
        private readonly TimeSpan _defaultTimeout;
        private readonly bool _ownTheGate;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock" /> class.
        /// </summary>
        /// <param name="gateLimit">The number of max concurrent requests (beyond this count request will delay until the completion of other request).
        /// </param>
        /// <param name="defaultTimeout">
        /// The default timeout will cause the waiting call to throw exception,
        /// Maximum waiting in order to acquires the lock-scope, beyond this waiting it will throw TimeoutException.  
        /// </param>
        public AsyncLock(TimeSpan defaultTimeout, ushort gateLimit = 1)
        {
            _gate = new SemaphoreSlim(gateLimit);
            _ownTheGate = true;
            _defaultTimeout = defaultTimeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock" /> class.
        /// </summary>
        /// <param name="gate">The gate.</param>
        /// <param name="defaultTimeout">The default timeout will cause the waiting call to throw exception,
        /// Maximum waiting in order to acquires the lock-scope, beyond this waiting it will throw TimeoutException.</param>
        public AsyncLock(SemaphoreSlim gate, TimeSpan defaultTimeout)
        {
            _gate = gate;
            _ownTheGate = false;
            _defaultTimeout = defaultTimeout;
        }

        #endregion // Ctor

        #region TryAcquireAsync

        /// <summary>
        /// Try to acquire async lock,
        /// when failed the LockScope.Acquired will equals false
        /// </summary>
        /// <param name="overrideTimeout">The override timeout.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// lock disposal and acquired indication
        /// </returns>
        public async Task<LockScope> TryAcquireAsync(
                                        TimeSpan overrideTimeout = default,
                                        CancellationToken cancellation = default)
        {
            var timeout = overrideTimeout == default ? _defaultTimeout : overrideTimeout;
            bool acquired = await _gate.WaitAsync(timeout, cancellation);            
            return new LockScope(_gate, acquired, this /* keep alive - avoid disposal */);
        }

        /// <summary>
        /// Try to acquire async lock,
        /// when failed the LockScope.Acquired will equals false
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// lock disposal and acquired indication
        /// </returns>
        public async Task<LockScope> TryAcquireAsync(CancellationToken cancellation)
        {
            bool acquired = await _gate.WaitAsync(_defaultTimeout, cancellation);            
            return new LockScope(_gate, acquired, this /* keep alive - avoid disposal */);
        }


        #endregion // TryAcquireAsync

        #region AcquireAsync

        /// <summary>
        /// Try to acquire async lock,
        /// when it will throw TimeoutException
        /// </summary>
        /// <param name="overrideTimeout">The override timeout.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// lock disposal
        /// </returns>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="TimeoutException">when acquire lock fail</exception>
        public async Task<IDisposable> AcquireAsync(
                                        TimeSpan overrideTimeout = default,
                                        CancellationToken cancellation = default)
        {
            var scope = await TryAcquireAsync(overrideTimeout, cancellation);
            if (!scope.Acquired)
            {
                scope.Dispose();
                throw new TimeoutException();
            }
            return scope;
        }

        /// <summary>
        /// Try to acquire async lock,
        /// when it will throw TimeoutException
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// lock disposal
        /// </returns>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="TimeoutException">when acquire lock fail</exception>
        public async Task<IDisposable> AcquireAsync(CancellationToken cancellation)
        {
            var scope = await TryAcquireAsync(_defaultTimeout, cancellation);
            if (!scope.Acquired)
            {
                scope.Dispose();
                throw new TimeoutException();
            }
            return scope;
        }

        #endregion // AcquireAsync

        #region Dispose Pattern

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(_ownTheGate)
                _gate?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        ~AsyncLock()
        {
            Dispose();
        }

        #endregion // Dispose Pattern

        #region EmptyDisposal

        /// <summary>
        /// Empty Disposal
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        private class EmptyDisposal : IDisposable
        {
            public static readonly IDisposable Default = new EmptyDisposal();
#pragma warning disable MS003 
            private EmptyDisposal() { }
            public void Dispose() { }
#pragma warning restore MS003 // Lines of Code does not follow metric rules.
        }

        #endregion // EmptyDisposal
    }
}
