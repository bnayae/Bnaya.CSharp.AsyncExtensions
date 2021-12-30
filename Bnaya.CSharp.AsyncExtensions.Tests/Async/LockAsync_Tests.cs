using System;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

// TODO: New Thread, Task.Factory.StartNew, Task.Run

namespace Bnaya.CSharp.AsyncExtensions.Tests
{

    [Trait("category", "ci")]
    public class LockAsync_Tests
    {
        #region AsyncLock_Test 

        [Fact]
        public async Task AsyncLock_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => AsyncLock_Test());
            await Task.WhenAll(tasks);
        }
        [Fact]
        public async Task AsyncLock_Test()
        {
            using (var locker = new AsyncLock(TimeSpan.FromMilliseconds(200)))
            {
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
                    Assert.False(fireForget.IsCompleted, "fireForget.IsCompleted");
                }
                await fireForget.WithTimeout(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                Assert.True(fireForget.IsCompleted, "IsCompleted");
                await fireForget;
            }
        }
        [Fact]
        public async Task AsyncLock_WithGcCollect_Test()
        {
            using (var locker = new AsyncLock(TimeSpan.FromMilliseconds(200)))
            {
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
        }

        #endregion // AsyncLock_Test

        #region AsyncLock_TryAcquireAsync_Stress_Test 

        [Fact]
        public async Task AsyncLock_TryAcquireAsync_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => AsyncLock_TryAcquireAsync_Test());
            await Task.WhenAll(tasks);
        }
        [Fact]
        public async Task AsyncLock_TryAcquireAsync_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
            Task fireForget;
            using (LockScope scope = await locker.TryAcquireAsync(TimeSpan.FromSeconds(5)))
            {
                Assert.True(scope.Acquired);
                fireForget = Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    using (LockScope scopeInner = await locker.TryAcquireAsync(TimeSpan.FromMilliseconds(50)))
                    {
                        sw.Stop();
                        // 48: timer may not be so accurate
                        Assert.True(sw.ElapsedMilliseconds >= 48, $"sw.ElapsedMilliseconds >= 50, Actual = {sw.ElapsedMilliseconds}");
                        Assert.False(scopeInner.Acquired, "scopeInner.Acquired");
                    }
                });
                await Task.Delay(100).ConfigureAwait(false);
            }
            await fireForget;
        }
        [Fact]
        public async Task AsyncLock_TryAcquireAsync_WithGcCollect_Test()
        {
            using (var locker = new AsyncLock(TimeSpan.FromMilliseconds(200)))
            {
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
        }
        [Fact]
        public async Task AsyncLock_TryAcquireAsync_Out_WithGcCollect_Test()
        {
            using (LockScope scope = await GetLockScope().ConfigureAwait(false))
            {
                CollectGC();
            }

            Task<LockScope> GetLockScope()
            {
                var locker = new AsyncLock(TimeSpan.FromMilliseconds(200));
                return locker.TryAcquireAsync(TimeSpan.FromSeconds(5));
                // locker can be disposed if not held
            }
        }

        #endregion // AsyncLock_TryAcquireAsync_Stress_Test

        #region SemaphoreSlim_AcquireAsync_Stress_Test 

        [Fact]
        public async Task SemaphoreSlim_AcquireAsync_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => SemaphoreSlim_AcquireAsync_Test());
            await Task.WhenAll(tasks);
        }
        [Fact]
        public async Task SemaphoreSlim_AcquireAsync_WithGcCollect_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 50)
                            .Select(_ => SemaphoreSlim_AcquireAsync_WithGcCollect_Test());
            await Task.WhenAll(tasks);
        }
        [Fact]
        public async Task SemaphoreSlim_AcquireAsync_Test()
        {
            using (var sync = new SemaphoreSlim(1))
            {
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
                    Assert.False(fireForget.IsCompleted, "fireForget.IsCompleted");
                }
                await Task.Delay(5).ConfigureAwait(false);
                Assert.True(fireForget.IsCompleted, "fireForget.IsCompleted");
                await fireForget;
            }
        }
        [Fact]
        public async Task SemaphoreSlim_AcquireAsync_WithGcCollect_Test()
        {
            using (var sync = new SemaphoreSlim(1))
            {
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
        }

        #endregion // SemaphoreSlim_AcquireAsync_Stress_Test

        #region SemaphoreSlim_TryAcquireAsync_Stress_Test 

        [Fact]
        public async Task SemaphoreSlim_TryAcquireAsync_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => SemaphoreSlim_TryAcquireAsync_Test());
            await Task.WhenAll(tasks);
        }
        [Fact]
        public async Task SemaphoreSlim_TryAcquireAsync_Test()
        {
            using (var sync = new SemaphoreSlim(1))
            {
                Task fireForget;
                using (LockScope scope = await sync.TryAcquireAsync(TimeSpan.FromSeconds(5)))
                {
                    Assert.True(scope.Acquired);
                    var sw = Stopwatch.StartNew();
                    fireForget = Task.Run(async () =>
                    {
                        using (LockScope scopeInner = await sync.TryAcquireAsync(TimeSpan.FromMilliseconds(50)))
                        {
                            sw.Stop();
                            Assert.True(sw.ElapsedMilliseconds >= 45, $"sw.ElapsedMilliseconds >= 50, Actual = {sw.ElapsedMilliseconds}");
                            Assert.False(scopeInner.Acquired, "scopeInner.Acquired");
                        }
                    });
                    await Task.Delay(100).ConfigureAwait(false);
                }
                await fireForget;
            }
        }

        #endregion // SemaphoreSlim_TryAcquireAsync_Stress_Test

        #region AsyncLock_Timeout_Test 

        [Fact]
        public async Task AsyncLock_Timeout_Stress_Test()
        {
            var tasks = Enumerable.Range(0, 1000)
                            .Select(_ => AsyncLock_Timeout_Test());
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task AsyncLock_Timeout_Test()
        {
            using (var locker = new AsyncLock(TimeSpan.FromMilliseconds(1)))
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
