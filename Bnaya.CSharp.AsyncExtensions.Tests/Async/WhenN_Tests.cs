using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public class WhenN_Tests
    {
        #region WhenN_OfT_Test

        [TestMethod]
        public async Task WhenN_OfT_Test()
        {
            // arrange
            var disposed = new ManualResetEventSlim(false);
            var sw = Stopwatch.StartNew();
            var sw1 = Stopwatch.StartNew();
            Task<int>[] tasks =
                     {
                        Delayed(TimeSpan.FromMilliseconds(1)),
                        Delayed(TimeSpan.FromMilliseconds(50)),
                        Delayed(TimeSpan.FromMilliseconds(100)),
                        Delayed(TimeSpan.FromMilliseconds(200))
                    };
            var cancellation = new CancellationTokenSource();

            // act
            (int[] results, bool succeed) = await tasks.WhenN(
                                                2,
                                                x => x != 50,
                                                cancellation,
                                                DisposeCancellation);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(100) &&
                          duration < TimeSpan.FromSeconds(10), "Duration");
            Assert.IsTrue(succeed, "Succeed");
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(100, results[1]);
            Assert.IsTrue(disposed.Wait(500), "DisposeCancellation");

            void DisposeCancellation(int[] _)
            {
                cancellation.Dispose();
                disposed.Set();
            }

            async Task<int> Delayed(TimeSpan time)
            {
                await Task.Delay(time).ConfigureAwait(false);
                return (int)time.TotalMilliseconds;
            }
        }

        #endregion // WhenN_OfT_Test

        #region WhenN_OfT_NoCondition_Test

        [TestMethod]
        public async Task WhenN_OfT_NoCondition_Test()
        {
            // arrange
            var disposed = new ManualResetEventSlim(false);
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
            (int[] results, bool succeed) = await tasks.WhenN(
                                                2,
                                                null,
                                                cancellation,
                                                DisposeCancellation);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(50) &&
                          duration < TimeSpan.FromSeconds(10), "Duration");
            Assert.IsTrue(succeed, "Succeed");
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(50, results[1]);
            Assert.IsTrue(disposed.Wait(500), "DisposeCancellation");

            void DisposeCancellation(int[] _)
            {
                cancellation.Dispose();
                disposed.Set();
            }

            async Task<int> Delayed(TimeSpan time)
            {
                await Task.Delay(time).ConfigureAwait(false);
                return (int)time.TotalMilliseconds;
            }
        }

        #endregion // WhenN_OfT_NoCondition_Test

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

            var disposed = new ManualResetEventSlim(false);

            // act
            (int[] results, bool succeed) = await tasks.WhenN(
                                                2,
                                                x => x == 1,
                                                cancellation,
                                                DisposeCancellation);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            Assert.IsFalse(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(200), "Duration");
            Assert.IsFalse(succeed, "Succeed");
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(1, results.Length, "Length");
            Assert.IsTrue(disposed.Wait(500), "DisposeCancellation");

            void DisposeCancellation(int[] _)
            {
                cancellation.Dispose();
                disposed.Set();
            }

            async Task<int> Delayed(TimeSpan time)
            {
                await Task.Delay(time).ConfigureAwait(false);
                return (int)time.TotalMilliseconds;
            }
        }

        #endregion // WhenN_NotSucceed_Test

        #region WhenN_Test

        [TestMethod]
        public async Task WhenN_Test()
        {
            // arrange
            var disposed = new ManualResetEventSlim(false);
            var sw = Stopwatch.StartNew();
            Task[] tasks =
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(1)),
                        Task.Delay(TimeSpan.FromMilliseconds(50)),
                        Task.Delay(TimeSpan.FromMilliseconds(100)),
                        Task.Delay(TimeSpan.FromMilliseconds(200))
                    };
            var cancellation = new CancellationTokenSource();

            // act
            await tasks.WhenN(2, cancellation,
                              DisposeCancellation);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(50) &&
                          duration < TimeSpan.FromSeconds(10), "Duration");
            Assert.IsTrue(disposed.Wait(500), "DisposeCancellation");

            void DisposeCancellation()
            {
                cancellation.Dispose();
                disposed.Set();
            }
        }

        #endregion // WhenN_Test
    }
}
