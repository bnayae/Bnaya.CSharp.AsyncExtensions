using System.ComponentModel;
using System.Reactive;
using System.Threading;

namespace System.Reactive.Linq
{
    /// <summary>
    /// Provides a set of static methods for subscribing delegates to observables.
    /// </summary>
    public static class WeakObservableExtensions
    {
        #region SubscribeWeak

        /// <summary>
        /// Weak Subscribes which wont prevent the observer from GC collecting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable">The observable.</param>
        /// <param name="observer">The observer.</param>
        /// <returns></returns>
        public static IDisposable SubscribeWeak<T>(
            this IObservable<T> observable,
            IObserver<T> observer)
        {
            var weak = new WeakObserver<T>(observer);
            return observable.Subscribe(weak);
        }

        #endregion // SubscribeWeak
    }
}