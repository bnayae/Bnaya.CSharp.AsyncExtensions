using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Thread-safe List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TapList<>.DebugView))]
    public class TapList<T> //: IAsyncEnumerable<T> // IList<T> IList is not TAP base API in nature
    {
        private List<T> _items;
        private readonly AsyncLock _gate;
        private readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(15);

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TapList{T}" /> class.
        /// </summary>
        /// <param name="timeout">The timeout (default = 15 seconds).</param>
        public TapList(TimeSpan timeout = default)
        {
            if (timeout == default)
                timeout = DEFAULT_TIMEOUT;
            _gate = new AsyncLock(timeout);
            _items = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TapList{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public TapList(IEnumerable<T> items, TimeSpan timeout = default)
        {
            if (timeout == default)
                timeout = DEFAULT_TIMEOUT;
            _gate = new AsyncLock(timeout);
            _items = new List<T>(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TapList{T}" /> class.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="items">The items.</param>
        public TapList(TimeSpan timeout, params T[] items)
            : this(items, timeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TapList{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public TapList(params T[] items)
            : this(items, default)
        {
        }

        #endregion // Ctor

        #region CountAsync

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns></returns>
        public async ValueTask<int> CountAsync(
            TimeSpan overrideDefaultTimeout = default)
        {
            using(await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                return _items.Count;
            }
        }

        #endregion // CountAsync

        #region AddAsync

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The added item
        /// </returns>
        public async ValueTask<T> AddAsync(
            T value,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.Add(value);
            }
            return value;
        }

        #endregion // AddAsync

        #region AddRangeAsync

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The added items
        /// </returns>
        public async ValueTask<IEnumerable<T>> AddRangeAsync(
            IEnumerable<T> values,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.AddRange(values);
            }
            return values;
        }

        #endregion // AddRangeAsync

        #region InsertAsync

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The inserted item.
        /// </returns>
        public async ValueTask<T> InsertAsync(
            int index,
            T item,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.Insert(index, item);
            }
            return item;
        }

        #endregion // InsertAsync

        #region RemoveAsync

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The removed value
        /// </returns>
        public async ValueTask<T> RemoveAsync(
            T value,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.Remove(value);
            }
            return value;
        }


        #endregion // RemoveAsync

        #region RemoveRangeAsync

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The count of the range.
        /// </returns>
        public async ValueTask<int> RemoveRangeAsync(
            int index,
            int count,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.RemoveRange(index, count);
            }
            return count;
        }

        #endregion // RemoveRangeAsync

        #region RemoveAtAsync

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The index
        /// </returns>
        public async ValueTask<int> RemoveAtAsync(
            int index,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.RemoveAt(index);
            }
            return index;
        }

        #endregion // RemoveAtAsync

        #region RemoveAllAsync

        /// <summary>
        /// Removes items according to condition.
        /// </summary>
        /// <param name="condition">The condition which should pass in order to remove the item.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>Count of removed items</returns>
        public async ValueTask<int> RemoveAllAsync(
            Predicate<T> condition,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                int count = _items.RemoveAll(condition);
                return count;
            }
        }

        #endregion // RemoveAllAsync

        #region ClearAsync

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>Count of removed items</returns>
        public async ValueTask<int> ClearAsync(
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                int count = _items.Count;
                _items.Clear();
                return count;
            }
        }

        #endregion // ClearAsync

        #region ContainsAsync

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public async ValueTask<bool> ContainsAsync(
            T item,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                bool contains = _items.Contains(item);
                return contains;
            }
        }

        #endregion // ContainsAsync

        #region CopyToAsync

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="fromIndexOnDest">The zero-based index on the destination array.
        /// The data of the source array will be fully copied
        /// into the destination, starts at the arrayIndex</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The destination array
        /// </returns>
        public async ValueTask<T[]> CopyToAsync(
            T[] array,
            int fromIndexOnDest,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                _items.CopyTo(array, fromIndexOnDest);
            }
            return array;
        }

        #endregion // CopyToAsync

        #region IndexOfAsync

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <param name="overrideDefaultTimeout">
        /// Override the default timeout (which can be set at construction time).
        /// </param>
        /// <returns>
        /// The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.
        /// </returns>
        public async ValueTask<int> IndexOfAsync(
            T item,
            TimeSpan overrideDefaultTimeout = default)
        {
            using (await _gate.AcquireAsync(overrideDefaultTimeout))
            {
                int index = _items.IndexOf(item);
                return index;
            }
        }

        #endregion // IndexOfAsync

        #region DebugView

        public class DebugView
        {
            private readonly TapList<T> _items;

            public DebugView(TapList<T> items)
            {
                _items = items;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public List<T> items => _items._items;
        }

        #endregion // DebugView
    }
}
