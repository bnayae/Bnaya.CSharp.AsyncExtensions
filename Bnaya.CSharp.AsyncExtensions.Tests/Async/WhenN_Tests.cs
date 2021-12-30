using System;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    
    public sealed class WhenN_Tests: IDisposable
    {
        private readonly AsyncLock _gate = new AsyncLock(TimeSpan.FromSeconds(10));

        #region When_Test

        [Fact]
        public Task When_Test() => When_Test_All(true);
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task When_Test_All(
            bool useCancellation)
        {
            var cancellation = useCancellation ? new CancellationTokenSource() : default;
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
                    Assert.True(cancellation.IsCancellationRequested, "Cancellation");
                Assert.True(duration >= TimeSpan.FromMilliseconds(50) &&
                              duration < TimeSpan.FromMilliseconds(100), "Duration");
                Assert.True(succeed, "Succeed");
                Assert.Equal(50, result);
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

        [Fact]
        public Task WhenN_OfT_Test() => WhenN_OfT_Test_All(2, true, true);
        [Theory]
        [InlineData(2, true, true)]
        [InlineData(2, false, true)]
        [InlineData(2, true, false)]
        [InlineData(2, false, false)]
        public async Task WhenN_OfT_Test_All(
            int threshold,
            bool useCondition,
            bool useCancellation)
        {
            var cancellation = useCancellation ? new CancellationTokenSource() : null;
            int minDuration = 50;
            Func<int, bool>? condition = null;
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
                    Assert.True(cancellation.IsCancellationRequested, "Cancellation");
                Assert.True(duration >= TimeSpan.FromMilliseconds(minDuration) &&
                              duration < TimeSpan.FromMilliseconds(200), "Duration");
                Assert.True(succeed, "Succeed");
                Assert.Equal(1, results[0]);
                Assert.Equal(minDuration, results[1]);
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

        [Fact]
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
            Assert.False(cancellation.IsCancellationRequested, "Cancellation");
            Assert.True(duration >= TimeSpan.FromMilliseconds(200), "Duration");
            Assert.False(succeed, "Succeed");
            Assert.Equal(1, results[0]);
            Assert.Single(results);
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

        [Fact]
        public Task WhenN_Test() => WhenN_Test_All(2, true);

        [Theory]
        [InlineData(2, true)]
        [InlineData(2, false)]
        public async Task WhenN_Test_All(
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
#pragma warning disable AsyncFixer05 // Implicit casting from Task<Task> to Task or awaiting Task<Task>
            Task all = await tasks
                .WhenN(threshold, cancellation)
                .ConfigureAwait(false);
#pragma warning restore AsyncFixer05 // Implicit casting from Task<Task> to Task or awaiting Task<Task>

            // assert
            sw.Stop();
            TimeSpan duration = sw.Elapsed;
            if(cancellation != null)
                Assert.True(cancellation.IsCancellationRequested, "Cancellation");
            Assert.True(duration >= TimeSpan.FromMilliseconds(50) &&
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
