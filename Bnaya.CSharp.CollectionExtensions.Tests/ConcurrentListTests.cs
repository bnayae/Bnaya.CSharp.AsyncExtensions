using System;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bnaya.CSharp.CollectionExtensions.Tests
{
    [TestClass]
    public class ConcurrentListTests
    {
        private const int STRESS_LEVEL = 10_000;
        private const int START_COUNT = 10;

        private ConcurrentList<int> _unitUnderTest;
        private readonly int[] EXPECTED = Enumerable.Range(1, STRESS_LEVEL).ToArray();
        private int[] ADDITIONS;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            _unitUnderTest = new ConcurrentList<int>(EXPECTED.Take(START_COUNT));
            ADDITIONS = EXPECTED.Skip(START_COUNT).ToArray();
        }

        #endregion // Setup

        #region Add_Test

        [TestMethod]
        public void Add_Test()
        {
            var stateAfetrOperation = _unitUnderTest.Add(5);

            Assert.AreSame(_unitUnderTest.ToImmutable(), stateAfetrOperation);
            Assert.IsTrue(stateAfetrOperation.SequenceEqual(_unitUnderTest));
        }

        #endregion // Add_Test

        #region Add_Stress_Test

        [TestMethod]
        public async Task Add_Stress_Test()
        {
            var tasks = ADDITIONS
                .Select(i => Task.Run(() =>
                         {
                             var stateAfetrOperation = _unitUnderTest.Add(i);
                             Assert.AreEqual(stateAfetrOperation.Last(), i);
                             return stateAfetrOperation;
                         }));

            await Task.WhenAll(tasks);
            Assert.AreEqual(_unitUnderTest.Count, EXPECTED.Length);
            var sorted = _unitUnderTest.OrderBy(m => m);
            Assert.IsTrue(sorted.SequenceEqual(EXPECTED));
        }

        #endregion // Add_Stress_Test
    }
}
