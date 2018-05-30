using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Thread-safe List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TdfList<>.DebugView))]
    public class TdfList<T> //: IAsyncEnumerable<T> // IList<T> IList is not TAP base API in nature
    {
        private List<T> _items;
        private readonly ActionBlock<Action> _sync = new ActionBlock<Action>(a => a());

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TdfList{T}" /> class.
        /// </summary>
        public TdfList()
        {
            _items = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TdfList{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public TdfList(IEnumerable<T> items)
        {
            _items = new List<T>(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TdfList{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public TdfList(params T[] items)
            : this((IEnumerable<T>)items)
        {
        }

        #endregion // Ctor

        #region CountAsync

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public Task<int> CountAsync()
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() => tcs.TrySetResult(_items.Count));
            return tcs.Task;
        }

        #endregion // CountAsync

        #region AddAsync

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Task AddAsync(T value)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.Add(value);
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // AddAsync

        #region AddRangeAsync

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public Task AddRangeAsync(IEnumerable<T> values)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.AddRange(values);
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // AddRangeAsync

        #region InsertAsync

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns></returns>
        public Task InsertAsync(
            int index,
            T item)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.Insert(index, item);
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // InsertAsync

        #region RemoveAsync

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Task<bool> RemoveAsync(T value)
        {
            var tcs = new TaskCompletionSource<bool>();
            _sync.Post(() =>
            {
                bool removed = _items.Remove(value);
                tcs.TrySetResult(removed);
            });
            return tcs.Task;
        }


        #endregion // RemoveAsync

        #region RemoveRangeAsync

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public Task RemoveRangeAsync(
            int index,
            int count)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.RemoveRange(index, count);
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // RemoveRangeAsync

        #region RemoveAtAsync

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns></returns>
        public Task RemoveAtAsync(int index)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.RemoveAt(index);
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // RemoveAtAsync

        #region RemoveAllAsync

        /// <summary>
        /// Removes items according to condition.
        /// </summary>
        /// <param name="condition">The condition which should pass in order to remove the item.</param>
        /// <returns>
        /// Count of removed items
        /// </returns>
        public Task<int> RemoveAllAsync(Predicate<T> condition)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                int count = _items.RemoveAll(condition);
                tcs.TrySetResult(count);
            });
            return tcs.Task;
        }

        #endregion // RemoveAllAsync

        #region ClearAsync

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <returns></returns>
        public Task ClearAsync()
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.Clear();
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // ClearAsync

        #region ContainsAsync

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public Task<bool> ContainsAsync(T item)
        {
            var tcs = new TaskCompletionSource<bool>();
            _sync.Post(() =>
            {
                bool contains = _items.Contains(item);
                tcs.TrySetResult(contains);
            });
            return tcs.Task;
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
        /// <returns></returns>
        public Task CopyToAsync(
            T[] array,
            int fromIndexOnDest)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                _items.CopyTo(array, fromIndexOnDest);
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        #endregion // CopyToAsync

        #region IndexOfAsync

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.
        /// </returns>
        public Task<int> IndexOfAsync(T item)
        {
            var tcs = new TaskCompletionSource<int>();
            _sync.Post(() =>
            {
                int index = _items.IndexOf(item);
                tcs.TrySetResult(index);
            });
            return tcs.Task;
        }

        #endregion // IndexOfAsync

        #region DebugView

        public class DebugView
        {
            private readonly TdfList<T> _items;

            public DebugView(TdfList<T> items)
            {
                _items = items;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public List<T> items => _items._items;
        }

        #endregion // DebugView
    }
}
