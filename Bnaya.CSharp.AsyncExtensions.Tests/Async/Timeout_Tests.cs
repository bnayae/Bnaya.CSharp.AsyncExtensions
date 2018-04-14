using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

// TODO: New Thread, Task.Factory.StartNew, Task.Run
#pragma warning disable CS0618 // Type or member is obsolete

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public class Timeout_Tests
    {
        #region IsTimeoutAsync_Test

        [TestMethod]
        public async Task IsTimeoutAsync_Test()
        {
            bool timedout = await Task.Delay(200)
                .IsTimeoutAsync(TimeSpan.FromMilliseconds(2)).ConfigureAwait(false);
            Assert.IsTrue(timedout);
        }

        #endregion // IsTimeoutAsync_Test

        #region IsTimeoutAsync_CompleteOnTime_Test

        [TestMethod]
        public async Task IsTimeoutAsync_CompleteOnTime_Test()
        {
            bool timedout = await Task.Delay(2)
                .IsTimeoutAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            Assert.IsFalse(timedout);
        }

        #endregion // IsTimeoutAsync_CompleteOnTime_Test

        #region WithTimeout_Test

        [TestMethod]
        public async Task WithTimeout_Test()
        {
            try
            {
                await Task.Delay(200)
                        .WithTimeout(TimeSpan.FromMilliseconds(2)).ConfigureAwait(false);
                throw new NotSupportedException("Not expected");
            }
            catch (TimeoutException)
            {
            }
        }

        #endregion // WithTimeout_Test

        #region WithTimeout_CompleteOnTime_Test

        [TestMethod]
        public async Task WithTimeout_CompleteOnTime_Test()
        {
            try
            {
                await Task.Delay(2)
                        .WithTimeout(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                throw new NotSupportedException("Not expected");
            }
        }

        #endregion // WithTimeout_CompleteOnTime_Test

        #region WithTimeout_CompleteOnTime_WithDefault_Test

        [TestMethod]
        public async Task WithTimeout_CompleteOnTime_WithDefault_Test()
        {
            try
            {
                await Task.Delay(2)
                        .WithTimeout().ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                throw new NotSupportedException("Not expected");
            }
        }

        #endregion // WithTimeout_CompleteOnTime_WithDefault_Test
 
        #region WithTimeout_OfT_Test

        [TestMethod]
        public async Task WithTimeout_OfT_Test()
        {
            try
            {
                await Task.Run(async () =>
                                {
                                    await Task.Delay(200);
                                    return 42;
                                })
                        .WithTimeout(TimeSpan.FromMilliseconds(2)).ConfigureAwait(false);
                throw new NotSupportedException("Not expected");
            }
            catch (TimeoutException)
            {
            }
        }

        #endregion // WithTimeout_OfT_Test

        #region WithTimeout_OfT_CompleteOnTime_Test

        [TestMethod]
        public async Task WithTimeout_OfT_CompleteOnTime_Test()
        {
            try
            {
                await Task.Run(async () =>
                {
                    await Task.Delay(2);
                    return 42;
                })
                        .WithTimeout(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                throw new NotSupportedException("Not expected");
            }
        }

        #endregion // WithTimeout_OfT_CompleteOnTime_Test
    }
}
