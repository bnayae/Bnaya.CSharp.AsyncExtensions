using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

// TODO: New Thread, Task.Factory.StartNew, Task.Run

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public class CancellationToken_Tests
    {
        #region CancellationToken_RegisterWeak_Test [Ignore]

        [TestMethod]
        [Ignore]
        public void CancellationToken_RegisterWeak_Test()
        {
            //var cts = new CancellationTokenSource();
            //var weak = Alloc();
            //Assert.IsTrue(weak.TryGetTarget(out var trg));
            //cts.Cancel();
            //Assert.IsTrue(trg.Called);
            //trg = null;
            //GC.Collect();
            //cts.Cancel();
            //Assert.IsFalse(weak.TryGetTarget(out var trg1));
            //WeakReference<RegTarget> Alloc()
            //{
            //    var target = new RegTarget();
            //    cts.Token.RegisterWeak(target.Handler);
            //    return new WeakReference<RegTarget>(target);
            //}
            //GC.KeepAlive(cts);
        }

        #endregion // CancellationToken_RegisterWeak_Test

        #region CancellationToken_Register_Test [Ignore] 

        [TestMethod]
        [Ignore]
        public void CancellationToken_Register_Test()
        {
            var cts = new CancellationTokenSource();
            var weak = Alloc();
            Assert.IsTrue(weak.TryGetTarget(out var trg));
            cts.Cancel();
            Assert.IsTrue(trg.Called);
            trg = null;
            GC.Collect();
            cts.Cancel();
            Assert.IsTrue(weak.TryGetTarget(out var trg1));
            WeakReference<RegTarget> Alloc()
            {
                var target = new RegTarget();
                cts.Token.Register(target.Handler);
                return new WeakReference<RegTarget>(target);
            }
            GC.KeepAlive(cts);
        }

        #endregion // CancellationToken_Register_Test

        #region CancellationToken_CancelSafe_NoError_Test

        [TestMethod]
        public void CancellationToken_CancelSafe_Test()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.Register(() => throw new Exception("X"));
            bool cancelled = cts.CancelSafe();
            Assert.IsFalse(cancelled);
        }

        [TestMethod]
        public void CancellationToken_CancelSafe_WithOutParameter_Test()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.Register(() => throw new Exception("X"));
            bool cancelled = cts.CancelSafe(out var e);
            Assert.IsFalse(cancelled);
            Assert.AreEqual("X", e.InnerException.Message);
        }

        [TestMethod]
        public void CancellationToken_CancelSafe_NoError_Test()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.Register(() => { });
            bool cancelled = cts.CancelSafe(out var e);
            Assert.IsTrue(cancelled);
        }

        #endregion // CancellationToken_CancelSafe_Test

        private class RegTarget
        {
            public bool Called { get; private set; }
            public void Handler() { Called = true; }
        }
    }
}
