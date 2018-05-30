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
    [DebuggerTypeProxy(typeof(LockedList<>.DebugView))]
    public class LockedList<T> //: IAsyncEnumerable<T> // IList<T> IList is not TAP base API in nature
    {
        private List<T> _items;
        private readonly object _gate = new object();

        #region Ctor

        /// <summary>lock(_gate)
        /// Initializes a new instance of the <see cref="LockedList{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public LockedList(IEnumerable<T> items)
        {
            _items = new List<T>(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockedList{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public LockedList(params T[] items)
            : this((IEnumerable<T>)items)
        {
        }

        #endregion // Ctor

        #region Count

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            lock(_gate)
            {
                return _items.Count;
            }
        }

        #endregion // Count

        #region Add

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(T value)
        {
            lock(_gate)
            {
                _items.Add(value);
            }
        }

        #endregion // Add

        #region AddRange

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>
        /// The added items
        /// </returns>
        public void AddRange(
            IEnumerable<T> values)
        {
            lock(_gate)
            {
                _items.AddRange(values);
            }
        }

        #endregion // AddRange

        #region Insert

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        public void Insert(
            int index,
            T item)
        {
            lock(_gate)
            {
                _items.Insert(index, item);
            }
        }

        #endregion // Insert

        #region Remove

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The removed value
        /// </returns>
        public bool Remove(T value)
        {
            lock(_gate)
            {
                return _items.Remove(value);
            }
        }


        #endregion // Remove

        #region RemoveRange

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The count of the range.
        /// </returns>
        public void RemoveRange(
            int index,
            int count)
        {
            lock(_gate)
            {
                _items.RemoveRange(index, count);
            }
        }

        #endregion // RemoveRange

        #region RemoveAt

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>
        /// The index
        /// </returns>
        public void RemoveAt(int index)
        {
            lock(_gate)
            {
                _items.RemoveAt(index);
            }
        }

        #endregion // RemoveAt

        #region RemoveAll

        /// <summary>
        /// Removes items according to condition.
        /// </summary>
        /// <param name="condition">The condition which should pass in order to remove the item.</param>
        /// <returns>Count of removed items</returns>
        public int RemoveAll(
            Predicate<T> condition)
        {
            lock(_gate)
            {
                int count = _items.RemoveAll(condition);
                return count;
            }
        }

        #endregion // RemoveAll

        #region Clear

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <returns>
        /// Count of removed items
        /// </returns>
        public int Clear()
        {
            lock(_gate)
            {
                int count = _items.Count;
                _items.Clear();
                return count;
            }
        }

        #endregion // Clear

        #region Contains

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            lock(_gate)
            {
                bool contains = _items.Contains(item);
                return contains;
            }
        }

        #endregion // Contains

        #region CopyTo

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="fromIndexOnDest">The zero-based index on the destination array.
        /// The data of the source array will be fully copied
        /// into the destination, starts at the arrayIndex</param>
        public void CopyTo(
            T[] array,
            int fromIndexOnDest)
        {
            lock(_gate)
            {
                _items.CopyTo(array, fromIndexOnDest);
            }
        }

        #endregion // CopyTo

        #region IndexOf

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            lock(_gate)
            {
                int index = _items.IndexOf(item);
                return index;
            }
        }

        #endregion // IndexOf

        #region DebugView

        public class DebugView
        {
            private readonly LockedList<T> _items;

            public DebugView(LockedList<T> items)
            {
                _items = items;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public List<T> items => _items._items;
        }

        #endregion // DebugView
    }
}
