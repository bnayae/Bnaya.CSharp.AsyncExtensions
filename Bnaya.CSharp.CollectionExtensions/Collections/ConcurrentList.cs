using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Thread-safe List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public class ConcurrentList<T> : IList<T>
    {
        private ImmutableList<T> _items;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        public ConcurrentList()
        {
            _items = ImmutableList<T>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public ConcurrentList(IEnumerable<T> items)
        {
            _items = ImmutableList<T>.Empty.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public ConcurrentList(params T[] items)
        {
            _items = ImmutableList.Create(items);
        }

        #endregion // Ctor

        #region Count

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public int Count => _items.Count;

        #endregion // Count

        #region IsReadOnly

        /// <summary>
        /// Gets a value indicating whether 
        /// the <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// is read-only.
        /// For getting readonly copy use ToImmutable()
        /// </summary>
        public bool IsReadOnly => false;

        #endregion // IsReadOnly

        #region T this[int index]

        /// <summary>
        /// Gets or sets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get => _items[index];
            set
            {
                while (true)
                {
                    var itemsCheck = _items;
                    var candidate = itemsCheck.RemoveAt(index).Insert(index, value);
                    if (TryAssign(itemsCheck, candidate))
                        break;
                }
            }
        }

        #endregion // T this[int index]

        #region Add

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        void ICollection<T>.Add(T value) => Add(value);

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// Immutable collection of the list state 
        /// right after the operation took effect.
        /// </returns>
        public ImmutableList<T> Add(T value)
        {
            while (true)
            {
                var itemsCheck = _items;
                var candidate = itemsCheck.Add(value);
                if (TryAssign(itemsCheck, candidate))
                    return candidate;
            }
        }

        #endregion // Add

        #region AddRange

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="values">The values.</param>
        public ImmutableList<T> AddRange(params T[] values) => AddRange((IEnumerable<T>)values);

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="values">The values.</param>
        public ImmutableList<T> AddRange(IEnumerable<T> values)
        {
            while (true)
            {
                var itemsCheck = _items;
                var candidate = itemsCheck.AddRange(values);
                if (TryAssign(itemsCheck, candidate))
                    return candidate;
            }
        }

        #endregion // AddRange

        #region Insert

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        void IList<T>.Insert(int index, T item) => Insert(index, item);

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        public ImmutableList<T> Insert(int index, T item)
        {
            while (true)
            {
                var itemsCheck = _items;
                var candidate = itemsCheck.Insert(index, item);
                if (TryAssign(itemsCheck, candidate))
                    return candidate;
            }
        }

        #endregion // Insert

        #region Remove

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if <paramref name="item">item</paramref> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        bool ICollection<T>.Remove(T item)
        {
            var items = _items;
            ImmutableList<T>  results = Remove(item);
            return results != items;
        }

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public ImmutableList<T> Remove(T value)
        {
            while (true)
            {
                var itemsCheck = _items;
                var candidate = itemsCheck.Remove(value);
                if (TryAssign(itemsCheck, candidate))
                    return candidate;
            }
        }

        #endregion // Remove

        #region RemoveRange

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="values">The values.</param>
        public ImmutableList<T> RemoveRange(params T[] values)
        {
            while (true)
            {
                var itemsCheck = _items;
                var candidate = itemsCheck.RemoveRange(values);
                if (TryAssign(itemsCheck, candidate))
                    return candidate;
            }
        }

        #endregion // RemoveRange

        #region RemoveAt

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="NotImplementedException"></exception>
        void IList<T>.RemoveAt(int index) => RemoveAt(index);

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="NotImplementedException"></exception>
        public ImmutableList<T> RemoveAt(int index)
        {
            while (true)
            {
                var itemsCheck = _items;
                var candidate = itemsCheck.RemoveAt(index);
                if (TryAssign(itemsCheck, candidate))
                    return candidate;
            }
        }

        #endregion // RemoveAt

        #region Clear

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        void ICollection<T>.Clear() => Clear();

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public ImmutableList<T> Clear()
        {
            while (true)
            {
                var candidate = ImmutableList<T>.Empty;
                if (TryAssign(_items, candidate))
                    return candidate;
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
        public bool Contains(T item) => _items.Contains(item);

        #endregion // Contains

        #region CopyTo

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) =>
                            _items.CopyTo(array, arrayIndex);

        #endregion // CopyTo

        #region IndexOf

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item) => _items.IndexOf(item);

        #endregion // IndexOf

        #region ToImmutable

        /// <summary>
        /// Gets to immutable.
        /// </summary>
        public IImmutableList<T> ToImmutable() => _items;

        #endregion // ToImmutable

        #region AsReadOnly

        /// <summary>
        /// Gets read-only (immutable) copy.
        /// </summary>
        public IReadOnlyList<T> AsReadOnly => _items;

        #endregion // AsReadOnly

        #region GetEnumerator

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion // GetEnumerator

        #region TryAssign

        /// <summary>
        /// Try to assign (thread-safety).
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        private bool TryAssign(ImmutableList<T> itemsCheck, ImmutableList<T> candidate)
        {
            ImmutableList<T> exchanged = Interlocked.CompareExchange(
                                                ref _items,
                                                candidate,
                                                itemsCheck);
            return itemsCheck == exchanged;
        }

        #endregion // TryAssign
    }
}
