using System;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

// TODO: New Thread, Task.Factory.StartNew, Task.Run

namespace Bnaya.CSharp.AsyncExtensions.Tests
{

    [Trait("category", "ci")]
    public class ThrowAll_Tests
    {
        #region ThrowAll_Task_Throw_Test

        [Fact]
        public async Task ThrowAll_Task_Throw_Test()
        {
            try
            {
                Task t1 = Task.Run(() => throw new Exception("A"));
                Task t2 = Task.Run(() => throw new Exception("B"));
                await Task.WhenAll(t1, t2).ThrowAll();
                throw new NotSupportedException("not expected");
            }
            catch (AggregateException ex)
            {
                var joinedMessages = string.Join("-", ex.InnerExceptions.Select(e => e.Message));
                Assert.Equal("A-B", joinedMessages);
            }
            catch (Exception)
            {
                throw new NotSupportedException("not expected");
            }

        }

        #endregion // ThrowAll_Task_Throw_Test

        #region ThrowAll_Task_Throw_Test

        [Fact]
        public async Task ThrowAll_TaskOfT_Throw_Test()
        {
            try
            {
                int i = 0;
                Task<int> t1 = Task.Run(() => 10 / i);
                Task<int> t2 = Task.Run(() => 10 / i);
                await Task.WhenAll(t1, t2).ThrowAll();
                throw new NotSupportedException("not expected");
            }
            catch (AggregateException ex)
            {
                var joinedMessages = string.Join("-", ex.InnerExceptions.Select(e => e.Message));
                Assert.Equal("Attempted to divide by zero.-Attempted to divide by zero.", joinedMessages);
            }
            catch (Exception)
            {
                throw new NotSupportedException("not expected");
            }

        }

        #endregion // ThrowAll_Task_Throw_Test

        #region ThrowAll_Task_Succeed_Test

        [Fact]
        public async Task ThrowAll_Task_Succeed_Test()
        {
            try
            {
                Task t1 = Task.Run(() => { });
                Task t2 = Task.Run(() => { });
                await Task.WhenAll(t1, t2).ThrowAll();
            }
            catch (AggregateException)
            {
                throw new NotSupportedException("not expected");
            }
            catch (Exception)
            {
                throw new NotSupportedException("not expected");
            }

        }

        #endregion // ThrowAll_Task_Succeed_Test

        #region ThrowAll_TaskOfT_Succeed_Test

        [Fact]
        public async Task ThrowAll_TaskOfT_Succeed_Test()
        {
            try
            {
                Task<int> t1 = Task.Run(() => 1);
                Task<int> t2 = Task.Run(() => 2);
                int[] arr = await Task.WhenAll(t1, t2).ThrowAll();
                arr.SequenceEqual(new[] { 1, 2 });
            }
            catch (AggregateException)
            {
                throw new NotSupportedException("not expected");
            }
            catch (Exception)
            {
                throw new NotSupportedException("not expected");
            }

        }

        #endregion // ThrowAll_TaskOfT_Succeed_Test
    }
}
