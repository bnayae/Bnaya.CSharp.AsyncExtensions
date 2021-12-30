using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Weak Event
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class WeakEvent<T> : IDisposable
    where T : class
{
    private readonly AsyncLock _sync = new AsyncLock(TimeSpan.FromSeconds(10));
    private readonly List<EquatableWeakReference> _subscribers = new List<EquatableWeakReference>();

    #region AddAsync

    /// <summary>
    /// Adds the asynchronous.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    private async Task AddAsync(Action<T> value)
    {
        using (await _sync.AcquireAsync())
        {
            var w = new EquatableWeakReference(value);
            _subscribers.Add(w);
        }
    }

    #endregion // AddAsync

    #region RemoveAsync

    /// <summary>
    /// Removes the asynchronous.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    private async Task RemoveAsync(Action<T> value)
    {
        using (await _sync.AcquireAsync())
        {
            for (int i = _subscribers.Count - 1; i >= 0; i--)
            {
                Action<T> item = _subscribers[i].Target;
                if (item == null && value == item)
                    _subscribers.RemoveAt(i);
            }
        }
    }

    #endregion // RemoveAsync

    #region Event

    /// <summary>
    /// Occurs when [week event].
    /// </summary>
    public event Action<T> Event
    {
        add
        {
            Task fireForget = AddAsync(value);
        }
        remove
        {
            Task fireForget = RemoveAsync(value);
        }
    }

    #endregion // Event

    #region Publish

    /// <summary>
    /// Publishes specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    public void Publish(T data)
    {
        Task fireForget = PublishAsync();

        async Task PublishAsync()
        {
            using (await _sync.AcquireAsync())
            {
                for (int i = _subscribers.Count - 1; i >= 0; i--)
                {
                    Action<T> item = _subscribers[i].Target;
                    if (item == null)
                        _subscribers.RemoveAt(i);
                    else
                        item(data);
                }
            }
        }
    }

    #endregion // Publish

    #region Dispose Pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _subscribers.Clear();
    }


    /// <summary>
    /// Finalizes an instance.
    /// </summary>
    ~WeakEvent()
    {
        Dispose();
    }

    #endregion // Dispose Pattern

    #region EquatableWeakReference

    /// <summary>
    /// Equatable Weak Reference
    /// </summary>
    [Serializable]
    private class EquatableWeakReference :
            WeakReference,
            IEquatable<Action<T>>,
            IEquatable<WeakReference<Action<T>>>,
            IEquatable<EquatableWeakReference>
    {
        private readonly static Action<T> Empty = (_) => { };

        #region Ctor

#pragma warning disable MS003 // Lines of Code does not follow metric rules.
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target">The object to track or null.</param>
        public EquatableWeakReference(object target) : base(target)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target">An object to track.</param>
        /// <param name="trackResurrection">Indicates when to stop tracking the object. If true, the object is tracked after finalization; if false, the object is only tracked until finalization.</param>
        public EquatableWeakReference(object target, bool trackResurrection) : base(target, trackResurrection)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize the current <see cref="T:System.WeakReference"></see> object.</param>
        /// <param name="context">(Reserved) Describes the source and destination of the serialized stream specified by info.</param>
        protected EquatableWeakReference(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
#pragma warning restore MS003 // Lines of Code does not follow metric rules.

        #endregion // Ctor

        #region TryGetTarget

        /// <summary>
        /// Tries the get target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public bool TryGetTarget(out Action<T> target)
        {
            target = Target;
            return target != null;
        }

        #endregion // TryGetTarget

        #region Target

        /// <summary>
        /// Gets or sets the object (the target) referenced by the current <see cref="T:System.WeakReference"></see> object.
        /// </summary>
        public new Action<T> Target => base.Target as Action<T> ?? Empty;

        #endregion // Target

        #region GetHashCode

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var target = base.Target;
            if (target == null)
                return -1;

            return target.GetHashCode();
        }

        #endregion // GetHashCode

        #region Equals

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) =>
            object.Equals(obj, base.Target);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Action<T> other)
        {
            if (other == null)
                return false;
            Action<T> target = this.Target;
            if (target == null)
                return false;
            return target.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(WeakReference<Action<T>> other)
        {
            Action<T> target = this.Target;
            if (target == null)
                return false;
            if (other.TryGetTarget(out var otherTarget))
                return target.Equals(otherTarget);
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(EquatableWeakReference other)
        {
            Action<T> target = this.Target;
            if (target == null)
                return false;
            if (other.TryGetTarget(out var otherTarget))
                return target.Equals(otherTarget);
            return false;
        }

        #endregion // Equals
    }

    #endregion // EquatableWeakReference
}