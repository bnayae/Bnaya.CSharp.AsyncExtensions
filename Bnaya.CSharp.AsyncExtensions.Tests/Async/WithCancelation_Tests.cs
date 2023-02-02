using System;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{

    [Trait("category", "ci")]
    public class WithCancelation_Tests
    {
        [Fact]
        public async Task WithCancelation_Pass_Test()
        {
            var cts = new CancellationTokenSource();
            await Task.Delay(1)
                      .WithCancellation(CancellationToken.None);
        }

        [Fact]
        public async Task WithCancelation_Canceled_Test()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(1)
                      .WithCancellation(cts.Token));
        }

        [Fact]
        public async Task OfT_WithCancelation_Pass_Test()
        {
            var result = await ExecAsync()
                      .WithCancellation(CancellationToken.None);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task OfT_WithCancelation_Canceled_Test()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1);
            await Assert.ThrowsAsync<TaskCanceledException>(() => ExecAsync(100)
                      .WithCancellation(cts.Token));
        }

        [Fact]
        public async Task OfT_WithCancelation_Canceled_With_Result_Test()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1);
            var result = await ExecAsync(100)
                      .WithCancellation(cts.Token, () => -42);
            Assert.Equal(-42, result);
        }


        private async Task<int> ExecAsync(int delay = 1)
        {
            await Task.Delay(delay);
            return 42;
        }
    }
}
