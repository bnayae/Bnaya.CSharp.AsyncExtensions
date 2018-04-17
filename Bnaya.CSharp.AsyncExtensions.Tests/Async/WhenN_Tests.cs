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
        private readonly AsyncLock _gate = new AsyncLock(TimeSpan.FromSeconds(10));

        #region When_Test

        [TestMethod]
        public async Task When_Test() => When_Test(true, true);
        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public async Task When_Test(
            bool useCancellation,
            bool useWhenAllCompleteAction)
        {
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            var disposed = new ManualResetEventSlim(false);
            Action<int[]> whenAllCompleteAction = null;
            if (useWhenAllCompleteAction)
                whenAllCompleteAction = DisposeCancellation;
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
                (int result, bool succeed) = await tasks.When(
                                                    x => x != 1,
                                                    cancellation,
                                                    whenAllCompleteAction);

                // assert
                sw.Stop();
                TimeSpan duration = sw.Elapsed;
                if (cancellation != null)
                    Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
                Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(50) &&
                              duration < TimeSpan.FromMilliseconds(100), "Duration");
                Assert.IsTrue(succeed, "Succeed");
                Assert.AreEqual(1, result);
                if (whenAllCompleteAction != null)
                    Assert.IsTrue(disposed.Wait(500), "DisposeCancellation");
            }
            void DisposeCancellation(int[] _)
            {
                cancellation?.Dispose();
                disposed.Set();
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
        public async Task WhenN_OfT_Test() => WhenN_OfT_Test(2, true, true, true);
        [DataTestMethod]
        [DataRow(2, true, true, true)]
        [DataRow(2, false, true, true)]
        [DataRow(2, true, false, true)]
        [DataRow(2, true, true, false)]
        [DataRow(2, true, false, false)]
        [DataRow(2, false, true, false)]
        [DataRow(2, false, false, true)]
        [DataRow(2, false, false, false)]
        public async Task WhenN_OfT_Test(
            int threshold,
            bool useCondition,
            bool useCancellation,
            bool useWhenAllCompleteAction)
        {
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            var disposed = new ManualResetEventSlim(false);
            int minDuration = 50;
            Func<int, bool> condition = null;
            if (useCondition)
            {
                condition = x => x != 50;
                minDuration = 100;
            }
            Action<int[]> whenAllCompleteAction = null;
            if (useWhenAllCompleteAction)
                whenAllCompleteAction = DisposeCancellation;
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
                (int[] results, bool succeed) = await tasks.WhenN(
                                                    threshold,
                                                    condition,
                                                    cancellation,
                                                    whenAllCompleteAction);

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
                if (whenAllCompleteAction != null)
                    Assert.IsTrue(disposed.Wait(500), "DisposeCancellation");
            }
            void DisposeCancellation(int[] _)
            {
                cancellation?.Dispose();
                disposed.Set();
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
        public async Task WhenN_Test() => WhenN_Test(2, true, true);
        [DataTestMethod]
        [DataRow(2, true, true)]
        [DataRow(2, false, true)]
        [DataRow(2, true, false)]
        [DataRow(2, true, true)]
        public async Task WhenN_Test(
            int threshold,
            bool useCancellation,
            bool useWhenAllCompleteAction)
        {
            // arrange
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            var disposed = new ManualResetEventSlim(false);
            Action whenAllCompleteAction = null;
            if (useWhenAllCompleteAction)
                whenAllCompleteAction = DisposeCancellation;
            var sw = Stopwatch.StartNew();
            Task[] tasks =
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(1)),
                        Task.Delay(TimeSpan.FromMilliseconds(50)),
                        Task.Delay(TimeSpan.FromMilliseconds(100)),
                        Task.Delay(TimeSpan.FromMilliseconds(200))
                    };
            // act
            await tasks.WhenN(threshold, cancellation,
                              DisposeCancellation);

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            Assert.IsTrue(cancellation.IsCancellationRequested, "Cancellation");
            Assert.IsTrue(duration >= TimeSpan.FromMilliseconds(50) &&
                          duration < TimeSpan.FromMilliseconds(100), "Duration");
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
