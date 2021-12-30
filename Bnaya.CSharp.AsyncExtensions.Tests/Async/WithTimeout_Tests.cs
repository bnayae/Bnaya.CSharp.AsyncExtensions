using System;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{

    [Trait("category", "ci")]
    public class WithTimeout_Tests
    {
        [Fact]
        public async Task WithTimeout_Pass_Test()
        {
            await Task.Delay(1)
                      .WithTimeout(TimeSpan.FromMilliseconds(100))
                      .ConfigureAwait(false);
        }
    }
}
