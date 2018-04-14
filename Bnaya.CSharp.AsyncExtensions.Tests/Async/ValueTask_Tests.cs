using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public class ValueTask_Tests
    {
        [TestMethod]
        public void ToValueTask_Test()
        {
            ValueTask<string> vt = "ABC".ToValueTask();
            Assert.AreEqual("ABC", vt.Result);
        }
    }
}
