using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public class WithTimeout_Tests
    {
        [TestMethod]
        public async Task WithTimeout_Pass_Test()
        {
            await Task.Delay(1)
                      .WithTimeout(TimeSpan.FromMilliseconds(100))
                      .ConfigureAwait(false);
        }
    }
}
