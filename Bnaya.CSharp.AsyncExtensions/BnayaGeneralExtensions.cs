using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Task Extensions
    /// </summary>
    public static class BnayaGeneralExtensions
    {
        #region Plural

        /// <summary>
        /// Plurals the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IEnumerable<T> Plural<T>(this T item)
        {
            yield return item;
        }

        #endregion // Plural

        #region AsWeak / Task

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <param name="action">The action.</param>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Action AsWeak(
            this Action action)
        {
            var weak = new WeakReference<Action>(action);
            Action result = () =>
            {
                Action act;
                if (weak.TryGetTarget(out act))
                    act();
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <param name="action">The action.</param>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Action<T> AsWeak<T>(
            this Action<T> action)
        {
            var weak = new WeakReference<Action<T>>(action);
            Action<T> result = m =>
            {
                Action<T> act;
                if (weak.TryGetTarget(out act))
                    act(m);
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Action<T1, T2> AsWeak<T1, T2>(
            this Action<T1, T2> action)
        {
            var weak = new WeakReference<Action<T1, T2>>(action);
            Action<T1, T2> result = (m1, m2) =>
            {
                Action<T1, T2> act;
                if (weak.TryGetTarget(out act))
                    act(m1, m2);
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Action<T1, T2, T3> AsWeak<T1, T2, T3>(
            this Action<T1, T2, T3> action)
        {
            var weak = new WeakReference<Action<T1, T2, T3>>(action);
            Action<T1, T2, T3> result = (m1, m2, m3) =>
            {
                Action<T1, T2, T3> act;
                if (weak.TryGetTarget(out act))
                    act(m1, m2, m3);
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Func<T1, TResult?> AsWeak<T1, TResult>(
            this Func<T1, TResult> action)
        {
            var weak = new WeakReference<Func<T1, TResult>>(action);
            Func<T1, TResult?> result = (m1) =>
            {
                Func<T1, TResult> act;
                if (weak.TryGetTarget(out act))
                    return act(m1);
                return default(TResult);
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Func<T1, T2, TResult?> AsWeak<T1, T2, TResult>(
            this Func<T1, T2, TResult> action)
        {
            var weak = new WeakReference<Func<T1, T2, TResult>>(action);
            Func<T1, T2, TResult?> result = (m1, m2) =>
            {
                Func<T1, T2, TResult> act;
                if (weak.TryGetTarget(out act))
                    return act(m1, m2);
                return default(TResult);
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action.</param>
        //[DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Func<T1, T2, T3, TResult?> AsWeak<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> action)
        {
            var weak = new WeakReference<Func<T1, T2, T3, TResult>>(action);
            Func<T1, T2, T3, TResult?> result = (m1, m2, m3) =>
            {
                Func<T1, T2, T3, TResult> act;
                if (weak.TryGetTarget(out act))
                    return act(m1, m2, m3);
                return default(TResult);
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// returns Task.CompletedTask when the func collected.
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Func<T, Task> AsWeakTask<T>(
            this Func<T, Task> action)
        {
            var weak = new WeakReference<Func<T, Task>>(action);
            Func<T, Task> result = m =>
            {
                Func<T, Task> act;
                if (weak.TryGetTarget(out act))
                    return act(m);
                return Task.CompletedTask;
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// returns Task.CompletedTask when the func collected.
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Func<T1, T2, Task> AsWeakTask<T1, T2>(
            this Func<T1, T2, Task> action)
        {
            var weak = new WeakReference<Func<T1, T2, Task>>(action);
            Func<T1, T2, Task> result = (m1, m2) =>
            {
                Func<T1, T2, Task> act;
                if (weak.TryGetTarget(out act))
                    return act(m1, m2);
                return Task.CompletedTask;
            };
            return result;
        }

        /// <summary>
        /// Make action a Weak reference.
        /// Consider it when registering to long running instance and
        /// when having capture variable.
        /// (important when the action is of instance which will keep the object alive).
        /// returns Task.CompletedTask when the func collected.
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static Func<T1, T2, T3, Task> AsWeakTask<T1, T2, T3>(
            this Func<T1, T2, T3, Task> action)
        {
            var weak = new WeakReference<Func<T1, T2, T3, Task>>(action);
            Func<T1, T2, T3, Task> result = (m1, m2, m3) =>
            {
                Func<T1, T2, T3, Task> act;
                if (weak.TryGetTarget(out act))
                    return act(m1, m2, m3);
                return Task.CompletedTask;
            };
            return result;
        }

        #endregion // AsWeak / Task
    }
}

