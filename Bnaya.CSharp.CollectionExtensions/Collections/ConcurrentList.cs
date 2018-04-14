using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace Bnaya.CSharp.AsyncExtensions.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public class ConcurrentList<T> : IEnumerable<T>
    {
        private ImmutableList<T> _items = ImmutableList<T>.Empty;

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(T value)
        {
            while (true)
            {
                ImmutableList<T> items = _items;
                ImmutableList<T> exchanged = Interlocked.CompareExchange(ref _items, items.Add(value), items);
                if (items == exchanged)
                    break;
            }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="values">The values.</param>
        public void AddRange(params T[] values)
        {
            while (true)
            {
                ImmutableList<T> items = _items;
                ImmutableList<T> exchanged = Interlocked.CompareExchange(ref _items, items.AddRange(values), items);
                if (items == exchanged)
                    break;
            }
        }

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Remove(T value)
        {
            while (true)
            {
                ImmutableList<T> items = _items;
                ImmutableList<T> exchanged = Interlocked.CompareExchange(ref _items, items.Remove(value), items);
                if (items == exchanged)
                    break;
            }
        }

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="values">The values.</param>
        public void RemoveRange(params T[] values)
        {
            while (true)
            {
                ImmutableList<T> items = _items;
                ImmutableList<T> exchanged = Interlocked.CompareExchange(ref _items, items.RemoveRange(values), items);
                if (items == exchanged)
                    break;
            }
        }

        /// <summary>
        /// Gets to immutable.
        /// </summary>
        public IImmutableList<T> ToImmutable => _items;

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
    }
}
