using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Xunit;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    public class ValueTask_Tests
    {
        [Fact]
        public void ToValueTask_Test()
        {
            ValueTask<string> vt = "ABC".ToValueTask();
            Assert.Equal("ABC", vt.Result);
        }
    }
}
