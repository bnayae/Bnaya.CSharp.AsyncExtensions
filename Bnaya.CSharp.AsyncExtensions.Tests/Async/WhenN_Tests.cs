using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public sealed class WhenN_Tests: IDisposable
    {
        private readonly AsyncLock _gate = new AsyncLock(TimeSpan.FromSeconds(10));

        #region When_Test

        [TestMethod]
        public Task When_Test() => When_Test(true);
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task When_Test(
            bool useCancellation)
        {
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            using (await _gate.AcquireAsync())
            {
                // arrange
                var sw = Stopwatch.StartNew();
                Task<int>[] tasks =
                         {
                        Delayed(TimeSpan.FromMilliseconds(1)),
                        Delayed(TimeSpan.FromMilliseconds(50)),
                        Delayed(TimeSpan.FromMilliseconds(100)),
                        Delayed(TimeSpan.FromMilliseconds(200))
                    };


                // act
                (int result, bool succeed, Task<int[]> all) = await tasks.When(
                                                    x => x != 1,
                                                    cancellation);

                // assert
                sw.Stop();
                TimeSpan duration = sw.Elapsed;
                if (cancellation != null)
                    Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
                Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(50) &&
                              duration < TimeSpan.FromMilliseconds(100), "Duration");
                Assert.IsTrue(succeed, "Succeed");
                Assert.AreEqual(50, result);
                await all.WithTimeout(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                cancellation?.Dispose();
            }

            async Task<int> Delayed(TimeSpan time)
            {
                await Task.Delay(time).ConfigureAwait(false);
                return (int)time.TotalMilliseconds;
            }
        }

        #endregion // When_Test

        #region WhenN_OfT_Test

        [TestMethod]
        public Task WhenN_OfT_Test() => WhenN_OfT_Test(2, true, true);
        [DataTestMethod]
        [DataRow(2, true, true)]
        [DataRow(2, false, true)]
        [DataRow(2, true, true)]
        [DataRow(2, false, false)]
        public async Task WhenN_OfT_Test(
            int threshold,
            bool useCondition,
            bool useCancellation)
        {
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            int minDuration = 50;
            Func<int, bool> condition = null;
            if (useCondition)
            {
                condition = x => x != 50;
                minDuration = 100;
            }
            using (await _gate.AcquireAsync())
            {
                // arrange
                var sw = Stopwatch.StartNew();
                Task<int>[] tasks =
                         {
                        Delayed(TimeSpan.FromMilliseconds(1)),
                        Delayed(TimeSpan.FromMilliseconds(50)),
                        Delayed(TimeSpan.FromMilliseconds(100)),
                        Delayed(TimeSpan.FromMilliseconds(200))
                    };


                // act
                (int[] results, bool succeed, Task<int[]> all) = await tasks.WhenN(
                                                    threshold,
                                                    condition,
                                                    cancellation);

                // assert
                sw.Stop();
                TimeSpan duration = sw.Elapsed;
                if (cancellation != null)
                    Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
                Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(minDuration) &&
                              duration < TimeSpan.FromMilliseconds(200), "Duration");
                Assert.IsTrue(succeed, "Succeed");
                Assert.AreEqual(1, results[0]);
                Assert.AreEqual(minDuration, results[1]);
                await all.WithTimeout(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                cancellation?.Dispose();
            }

            async Task<int> Delayed(TimeSpan time)
            {
                await Task.Delay(time).ConfigureAwait(false);
                return (int)time.TotalMilliseconds;
            }
        }

        #endregion // WhenN_OfT_Test

        #region WhenN_OfT_NotSucceed_Test

        [TestMethod]
        public async Task WhenN_OfT_NotSucceed_Test()
        {
            // arrange
            var sw = Stopwatch.StartNew();
            Task<int>[] tasks =
                    {
                        Delayed(TimeSpan.FromMilliseconds(1)),
                        Delayed(TimeSpan.FromMilliseconds(50)),
                        Delayed(TimeSpan.FromMilliseconds(100)),
                        Delayed(TimeSpan.FromMilliseconds(200))
                    };
            var cancellation = new CancellationTokenSource();

            // act
            (int[] results, bool succeed, Task<int[]> all) = await tasks.WhenN(
                                                2,
                                                x => x == 1,
                                                cancellation);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            Assert.IsFalse(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(200), "Duration");
            Assert.IsFalse(succeed, "Succeed");
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(1, results.Length, "Length");
            await all.WithTimeout(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            cancellation?.Dispose();

            async Task<int> Delayed(TimeSpan time)
            {
                await Task.Delay(time).ConfigureAwait(false);
                return (int)time.TotalMilliseconds;
            }
        }

        #endregion // WhenN_NotSucceed_Test

        #region WhenN_Test

        [TestMethod]
        public Task WhenN_Test() => WhenN_Test(2, true);
        [DataTestMethod]
        [DataRow(2, true)]
        [DataRow(2, false)]
        public async Task WhenN_Test(
            int threshold,
            bool useCancellation)
        {
            // arrange
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            var sw = Stopwatch.StartNew();
            Task[] tasks =
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(1)),
                        Task.Delay(TimeSpan.FromMilliseconds(50)),
                        Task.Delay(TimeSpan.FromMilliseconds(100)),
                        Task.Delay(TimeSpan.FromMilliseconds(200))
                    };
            // act
            Task all = await tasks
                .WhenN(threshold, cancellation)
                .ConfigureAwait(false);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            if(cancellation != null)
                Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(50) &&
                          duration < TimeSpan.FromMilliseconds(100), "Duration");
            await all.WithTimeout(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            cancellation?.Dispose();
        }


        #endregion // WhenN_Test

        #region Dispose

        public void Dispose()
        {
            _gate.Dispose();
        }
 
        #endregion // Dispose
   }
}
