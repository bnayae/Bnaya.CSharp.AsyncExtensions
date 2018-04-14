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
        public async Task AsyncLock_Test()
        {
            var locker = new AsyncLock(TimeSpan.FromMilliseconds(1));
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
                Assert.IsFalse(fireForget.IsCompleted);
            }
            await Task.Delay(5).ConfigureAwait(false);
            Assert.IsTrue(fireForget.IsCompleted);
        }

        #endregion // AsyncLock_Test

        #region AsyncLock_Timeout_Test 

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
    }
}
