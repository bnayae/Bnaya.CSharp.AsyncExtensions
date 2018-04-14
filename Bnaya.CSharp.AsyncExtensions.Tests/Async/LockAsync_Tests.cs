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
    public class LockAsync_Tests
    {
        #region AsyncLock_Test 

        [TestMethod]
        public async Task AsyncLock_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => AsyncLock_Test());
            await Task.WhenAll(tasks);
        }
        [TestMethod]
        public async Task AsyncLock_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
            Task fireForget;
            using (await locker.AcquireAsync())
            {
                fireForget = Task.Run(async () =>
                {
                    using (await locker.AcquireAsync())
                    {
                    }
                });
                await Task.Delay(5).ConfigureAwait(false);
                Assert.IsFalse(fireForget.IsCompleted, "fireForget.IsCompleted");
            }
            await Task.Delay(5).ConfigureAwait(false);
            Assert.IsTrue(fireForget.IsCompleted);
            await fireForget;
        }
        [TestMethod]
        public async Task AsyncLock_WithGcCollect_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
            Task fireForget;
            using (await locker.AcquireAsync())
            {
                CollectGC();
                fireForget = Task.Run(async () =>
                {
                    await Task.Delay(30);
                    using (await locker.AcquireAsync())
                    {
                    }
                });
            }
            CollectGC();
            await fireForget;
        }

        #endregion // AsyncLock_Test

        #region AsyncLock_TryAcquireAsync_Stress_Test 

        [TestMethod]
        public async Task AsyncLock_TryAcquireAsync_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => AsyncLock_TryAcquireAsync_Test());
            await Task.WhenAll(tasks);
        }
        [TestMethod]
        public async Task AsyncLock_TryAcquireAsync_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
            Task fireForget;
            using (LockScope scope = await locker.TryAcquireAsync(TimeSpan.FromSeconds(5)))
            {
                Assert.IsTrue(scope.Acquired);
                var sw = Stopwatch.StartNew();
                fireForget = Task.Run(async () =>
                {
                    using (LockScope scopeInner = await locker.TryAcquireAsync(TimeSpan.FromMilliseconds(50)))
                    {
                        sw.Stop();
                        Assert.IsTrue(sw.ElapsedMilliseconds >= 50);
                        Assert.IsFalse(scopeInner.Acquired);

                    }
                });
                await Task.Delay(100).ConfigureAwait(false);
            }
            await fireForget;
        }
        [TestMethod]
        public async Task AsyncLock_TryAcquireAsync_WithGcCollect_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
            Task fireForget;
            using (LockScope scope = await locker.TryAcquireAsync(TimeSpan.FromSeconds(5)))
            {
                CollectGC();
                fireForget = Task.Run(async () =>
                {
                    await Task.Delay(30);
                    using (LockScope scopeInner = await locker.TryAcquireAsync(TimeSpan.FromMilliseconds(50)))
                    {
                    }
                });
            }
            CollectGC();
            await fireForget;
        }
        [TestMethod]
        public async Task AsyncLock_TryAcquireAsync_Out_WithGcCollect_Test()
        {
            LockScope scope = await GetLockScope().ConfigureAwait(false);
            CollectGC();
            scope.Dispose();

            Task<LockScope> GetLockScope()
            {
                var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
                return locker.TryAcquireAsync(TimeSpan.FromSeconds(5));
                // locker can be disposed if not held
            }
        }

        #endregion // AsyncLock_TryAcquireAsync_Stress_Test

        #region SemaphoreSlim_AcquireAsync_Stress_Test 

        [TestMethod]
        public async Task SemaphoreSlim_AcquireAsync_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => SemaphoreSlim_AcquireAsync_Test());
            await Task.WhenAll(tasks);
        }
        [TestMethod]
        public async Task SemaphoreSlim_AcquireAsync_WithGcCollect_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 50)
                            .Select(_ => SemaphoreSlim_AcquireAsync_WithGcCollect_Test());
            await Task.WhenAll(tasks);
        }
        [TestMethod]
        public async Task SemaphoreSlim_AcquireAsync_Test()
        {
            var sync = new SemaphoreSlim(1);
            Task fireForget;
            using (await sync.AcquireAsync(TimeSpan.FromSeconds(5)))
            {
                fireForget = Task.Run(async () =>
                {
                    using (await sync.AcquireAsync(TimeSpan.FromSeconds(5)))
                    {
                    }
                });
                await Task.Delay(5).ConfigureAwait(false);
                Assert.IsFalse(fireForget.IsCompleted, "fireForget.IsCompleted");
            }
            await Task.Delay(5).ConfigureAwait(false);
            Assert.IsTrue(fireForget.IsCompleted);
            await fireForget;
        }
        [TestMethod]
        public async Task SemaphoreSlim_AcquireAsync_WithGcCollect_Test()
        {
            var sync = new SemaphoreSlim(1);
            Task fireForget;
            using (await sync.AcquireAsync(TimeSpan.FromSeconds(5)))
            {
                CollectGC();
                fireForget = Task.Run(async () =>
                {
                    await Task.Delay(30);
                    using (await sync.AcquireAsync(TimeSpan.FromSeconds(5)))
                    {
                    }
                });
            }
            CollectGC();
            await fireForget;
        }

        #endregion // SemaphoreSlim_AcquireAsync_Stress_Test

        #region SemaphoreSlim_TryAcquireAsync_Stress_Test 

        [TestMethod]
        public async Task SemaphoreSlim_TryAcquireAsync_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => SemaphoreSlim_TryAcquireAsync_Test());
            await Task.WhenAll(tasks);
        }
        [TestMethod]
        public async Task SemaphoreSlim_TryAcquireAsync_Test()
        {
            var sync = new SemaphoreSlim(1);
            Task fireForget;
            using (LockScope scope = await sync.TryAcquireAsync(TimeSpan.FromSeconds(5)))
            {
                Assert.IsTrue(scope.Acquired);
                var sw = Stopwatch.StartNew();
                fireForget = Task.Run(async () =>
                {
                    using (LockScope scopeInner = await sync.TryAcquireAsync(TimeSpan.FromMilliseconds(50)))
                    {
                        sw.Stop();
                        Assert.IsTrue(sw.ElapsedMilliseconds >= 50);
                        Assert.IsFalse(scopeInner.Acquired);

                    }
                });
                await Task.Delay(100).ConfigureAwait(false);
            }
            await fireForget;
        }

        #endregion // SemaphoreSlim_TryAcquireAsync_Stress_Test

        #region AsyncLock_Timeout_Test 

        [TestMethod]
        public async Task AsyncLock_Timeout_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => AsyncLock_Timeout_Test());
            await Task.WhenAll(tasks);
        }

        [TestMethod]
        public async Task AsyncLock_Timeout_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(1));
            using (await locker.AcquireAsync())
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        using (await locker.AcquireAsync(TimeSpan.FromMilliseconds(1)))
                        {
                        }
                        throw new NotSupportedException("Not expected");
                    }
                    catch (TimeoutException)
                    {
                        // expected
                    }
                });
            }
        }

        #endregion // AsyncLock_Timeout_Test

        #region CollectGC

        private void CollectGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #endregion // CollectGC
    }
}
