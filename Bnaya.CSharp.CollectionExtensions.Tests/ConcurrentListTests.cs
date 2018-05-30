using System;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bnaya.CSharp.CollectionExtensions.Tests
{
    [TestClass]
    public class ConcurrentListTests
    {
        private const int STRESS_LEVEL = 10_000;
        private const int START_COUNT = 10;
        private const int RANGE_SIZE = 100;

        private ConcurrentImmutableList<int> _unitUnderTest;
        private ConcurrentImmutableList<int> _unitUnderTestEmpty = new ConcurrentImmutableList<int>();
        private ConcurrentImmutableList<int> _unitUnderTestLarge;
        private readonly int[] EXPECTED = Enumerable.Range(1, STRESS_LEVEL).ToArray();
        private int[] ADDITIONS;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            _unitUnderTest = new ConcurrentImmutableList<int>(EXPECTED.Take(START_COUNT));
            _unitUnderTestLarge = new ConcurrentImmutableList<int>(EXPECTED);
            ADDITIONS = EXPECTED.Skip(START_COUNT).ToArray();
        }

        #endregion // Setup

        #region Add_Test

        [TestMethod]
        public void Add_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            var stateAfetrOperation = _unitUnderTest.Add(5);

            Assert.AreSame(_unitUnderTest.ToImmutable(), stateAfetrOperation);
            Assert.IsTrue(stateAfetrOperation.SequenceEqual(_unitUnderTest));
            Assert.IsTrue(snapshot.Add(5).SequenceEqual(_unitUnderTest));
            Assert.AreNotEqual(snapshot, stateAfetrOperation);
        }

        #endregion // Add_Test

        #region Add_Stress_Test

        [TestMethod]
        public async Task Add_Stress_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
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
            Assert.IsTrue(snapshot.SequenceEqual(
               _unitUnderTest.Take(snapshot.Count)));
        }

        #endregion // Add_Stress_Test

        #region AddRange_Test

        [TestMethod]
        public void AddRange_Test()
        {
            var stateAfetrOperation = _unitUnderTest.AddRange(ADDITIONS);

            Assert.AreEqual(_unitUnderTest.Count, EXPECTED.Length);
            Assert.AreEqual(stateAfetrOperation, _unitUnderTest.ToImmutable());
            Assert.IsTrue(_unitUnderTest.SequenceEqual(EXPECTED));
        }

        #endregion // AddRange_Test

        #region AddRange_Stress_Test

        [TestMethod]
        public async Task AddRange_Stress_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            var tasks = EXPECTED
                .Select(i => Task.Run(() =>
                {
                    var range = Enumerable.Range(i, RANGE_SIZE).ToArray();
                    var stateAfetrOperation = _unitUnderTestEmpty.AddRange(range);
                    Assert.AreEqual(stateAfetrOperation.Last(), i + RANGE_SIZE - 1);
                    return stateAfetrOperation;
                }));

            await Task.WhenAll(tasks);
            Assert.AreEqual(_unitUnderTestEmpty.Count, EXPECTED.Length * RANGE_SIZE);
            var sorted = _unitUnderTestEmpty.OrderBy(m => m);
            var expected = (from i in EXPECTED
                            from j in Enumerable.Range(i, RANGE_SIZE)
                            select j).OrderBy(m => m);
            Assert.IsTrue(sorted.SequenceEqual(expected));
            Assert.IsTrue(snapshot.SequenceEqual(
                       _unitUnderTest.Take(snapshot.Count)));
        }

        #endregion // AddRange_Stress_Test

        #region Insert_Test

        [TestMethod]
        public void Insert_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            var stateAfetrOperation = _unitUnderTest.Insert(0, 5);

            Assert.AreSame(_unitUnderTest.ToImmutable(), stateAfetrOperation);
            Assert.IsTrue(stateAfetrOperation.SequenceEqual(_unitUnderTest));
            Assert.IsTrue(snapshot.Insert(0, 5).SequenceEqual(_unitUnderTest));
            Assert.AreNotEqual(snapshot, stateAfetrOperation);
        }

        #endregion // Insert_Test

        #region Insert_Stress_Test

        [TestMethod]
        public async Task Insert_Stress_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            var tasks = ADDITIONS
                .Select(i => Task.Run(() =>
                         {
                             var stateAfetrOperation = _unitUnderTest.Insert(0, i);
                             Assert.AreEqual(stateAfetrOperation.First(), i);
                             return stateAfetrOperation;
                         }));

            await Task.WhenAll(tasks);
            Assert.AreEqual(_unitUnderTest.Count, EXPECTED.Length);
            var sorted = _unitUnderTest.OrderBy(m => m);
            Assert.IsTrue(sorted.SequenceEqual(EXPECTED));
            Assert.IsTrue(snapshot.SequenceEqual(
                        _unitUnderTest.Skip(_unitUnderTest.Count - snapshot.Count)));
        }

        #endregion // Insert_Stress_Test

        #region Remove_Test

        [TestMethod]
        public void Remove_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            var stateAfetrOperation = _unitUnderTest.Remove(_unitUnderTest.First());

            Assert.AreSame(_unitUnderTest.ToImmutable(), stateAfetrOperation);
            Assert.IsTrue(stateAfetrOperation.SequenceEqual(_unitUnderTest));
            Assert.IsTrue(snapshot.Skip(1).SequenceEqual(_unitUnderTest));
            Assert.AreNotEqual(snapshot, stateAfetrOperation);
        }

        #endregion // Remove_Test

        #region Remove_Stress_Test

        [TestMethod]
        public async Task Remove_Stress_Test()
        {
            var snapshot = _unitUnderTestLarge.ToImmutable();
            var tasks = Enumerable.Range(1, _unitUnderTestLarge.Count / 2)
                .Select(i => Task.Run(() => _unitUnderTestLarge.Remove(i)));

            await Task.WhenAll(tasks);
            Assert.AreEqual(_unitUnderTestLarge.Count, snapshot.Count / 2);
            Assert.IsTrue(_unitUnderTestLarge.SequenceEqual(snapshot.Skip(snapshot.Count / 2)));
        }

        #endregion // Remove_Stress_Test

        #region RemoveAt_Test

        [TestMethod]
        public void RemoveAt_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            var stateAfetrOperation = _unitUnderTest.RemoveAt(0);

            Assert.AreSame(_unitUnderTest.ToImmutable(), stateAfetrOperation);
            Assert.IsTrue(stateAfetrOperation.SequenceEqual(_unitUnderTest));
            Assert.IsTrue(snapshot.Skip(1).SequenceEqual(_unitUnderTest));
            Assert.AreNotEqual(snapshot, stateAfetrOperation);
        }

        #endregion // RemoveAt_Test

        #region RemoveAt_Stress_Test

        [TestMethod]
        public async Task RemoveAt_Stress_Test()
        {
            var snapshot = _unitUnderTestLarge.ToImmutable();
            var tasks = Enumerable.Range(0, _unitUnderTestLarge.Count / 2)
                .Select(i => Task.Run(() => _unitUnderTestLarge.RemoveAt(i)));

            await Task.WhenAll(tasks);
            Assert.AreEqual(_unitUnderTestLarge.Count, snapshot.Count / 2);
        }

        #endregion // RemoveAt_Stress_Test

        #region RemoveRange_Test

        [TestMethod]
        public void RemoveRange_Test()
        {
            var stateAfetrOperation = _unitUnderTest.RemoveRange(_unitUnderTest.Skip(2).ToArray());

            Assert.AreEqual(_unitUnderTest.Count, 2);
            Assert.AreEqual(stateAfetrOperation, _unitUnderTest.AsReadOnly());
            Assert.IsTrue(_unitUnderTest.SequenceEqual(EXPECTED.Take(2)));
        }

        #endregion // RemoveRange_Test

        #region RemoveRange_Stress_Test

        [TestMethod]
        public async Task RemoveRange_Stress_Test()
        {
            int[] removeFactors = { 3, 5, 7, 11, 17 };

            var tasks = removeFactors
                .Select(i => Task.Run(() =>
                {
                    var range = EXPECTED.Where(m => m % i == 0);
                    var stateAfetrOperation = _unitUnderTestLarge.RemoveRange(range);
                    return stateAfetrOperation;
                }));

            await Task.WhenAll(tasks);
            foreach (var factor in removeFactors)
            {
                Assert.IsFalse(_unitUnderTestLarge.Any(m => m % factor == 0));
            }
        }

        #endregion // RemoveRange_Stress_Test

        #region RemoveAll_Test

        [TestMethod]
        public void RemoveAll_Test()
        {
            var stateAfetrOperation = _unitUnderTest.RemoveAll(m => m > 2);

            Assert.AreEqual(_unitUnderTest.Count, 2);
            Assert.AreEqual(stateAfetrOperation, _unitUnderTest.AsReadOnly());
            Assert.IsTrue(_unitUnderTest.SequenceEqual(EXPECTED.Take(2)));
        }

        #endregion // RemoveAll_Test

        #region RemoveAll_Stress_Test

        [TestMethod]
        public async Task RemoveAll_Stress_Test()
        {
            int[] removeFactors = { 3, 5, 7 , 11, 17};

            var tasks = removeFactors
                .Select(i => Task.Run(() =>
                {
                    var stateAfetrOperation = _unitUnderTestLarge.RemoveAll(m => m % i == 0);
                    return stateAfetrOperation;
                }));

            await Task.WhenAll(tasks);
            foreach (var factor in removeFactors)
            {
                Assert.IsFalse(_unitUnderTestLarge.Any(m => m % factor == 0));
            }
        }

        #endregion // RemoveAll_Stress_Test
 
        #region Indexer_Test

        [TestMethod]
        public void Indexer_Test()
        {
            var snapshot = _unitUnderTest.ToImmutable();
            int i = _unitUnderTest[2];
            _unitUnderTest[2] = -1;
            Assert.AreEqual(_unitUnderTest[2], -1);
            Assert.AreNotEqual(_unitUnderTest.ToImmutable(), snapshot);
        }

        #endregion // Indexer_Test

        #region Indexer_Stress_Test

        [TestMethod]
        public async Task Indexer_Stress_Test()
        {
            var tasks = _unitUnderTestLarge
                .Select((v,i) => Task.Run(() =>
                {
                    if (v % 2 == 0)
                        _unitUnderTestLarge[i] += _unitUnderTestLarge[i];
                    else
                        _unitUnderTestLarge[i] = -_unitUnderTestLarge[i];
                }));

            await Task.WhenAll(tasks);
            for (int i = 0; i < _unitUnderTestLarge.Count; i++)
            {
                var item = _unitUnderTestLarge[i];
                if (item % 2 == 0)
                    Assert.AreEqual(item, (i + 1) * 2);
                else
                    Assert.AreEqual(item, -(i + 1));
            }
        }

        #endregion // Indexer_Stress_Test

        #region Clear_Test

        [TestMethod]
        public void Clear_Test()
        {
            var stateAfetrOperation = _unitUnderTest.Clear();

            Assert.AreSame(_unitUnderTest.ToImmutable(), stateAfetrOperation);
            Assert.AreSame(_unitUnderTest.ToImmutable(), ImmutableList<int>.Empty);
        }

        #endregion // Clear_Test

        #region Clear_Stress_Test

        [TestMethod]
        public async Task Clear_Stress_Test()
        {
            var tasks = _unitUnderTestLarge
                .Select(i => Task.Run(() =>
                {
                    if (_unitUnderTestLarge.Count == 0)
                        _unitUnderTestLarge.Add(i);
                    else
                        _unitUnderTestLarge.Clear();
                }));

            await Task.WhenAll(tasks);
            Assert.IsTrue(_unitUnderTestLarge.Count < Environment.ProcessorCount * 2);
        }

        #endregion // Clear_Stress_Test

        #region Contains_Test

        [TestMethod]
        public void Contains_Test()
        {
            Assert.IsTrue(_unitUnderTest.Contains(_unitUnderTest.First()));
            Assert.IsFalse(_unitUnderTest.Contains(-1));
        }

        #endregion // Contains_Test

        #region Contains_Stress_Test

        [TestMethod]
        public async Task Contains_Stress_Test()
        {
            var tasks = _unitUnderTestLarge
                .Select((v,i) => Task.Run(() =>
                {
                    if (i % 2 == 0)
                        return _unitUnderTestLarge.Contains(v);
                    else
                        return _unitUnderTestLarge.Contains(-v);
                }));

            bool[] results = await Task.WhenAll(tasks);
            Assert.IsTrue(results.Where((v, i) => i % 2 == 0).All(m => m));
            Assert.IsFalse(results.Where((v, i) => i % 2 != 0).Any(m => m));
        }

        #endregion // Contains_Stress_Test

        #region CopyTo_Test

        [TestMethod]
        public void CopyTo_Test()
        {
            int count = _unitUnderTest.Count;
            int dbl = count * 2;
            int[] arr = new int[dbl];
            var result = _unitUnderTest.CopyTo(arr, count);

            Assert.IsTrue(_unitUnderTest.SequenceEqual(arr.Skip(count)));
        }

        #endregion // CopyTo_Test

        #region CopyTo_Stress_Test

        [TestMethod]
        public async Task CopyTo_Stress_Test()
        {
            var tasks = _unitUnderTestLarge
                .Select(i => Task.Run(() =>
                {
                    var snap = _unitUnderTestLarge.RemoveAt(0);
                    var arr = new int[snap.Count];
                    snap = _unitUnderTestLarge.CopyTo(arr, 0);
                    return (Snapshot: snap, Array: arr.Take(snap.Count));
                }));

            var results = await Task.WhenAll(tasks);
            foreach (var result in results)
            {
                Assert.IsTrue(result.Snapshot.SequenceEqual(result.Array));
            }
        }

        #endregion // CopyTo_Stress_Test

        #region IndexOf_Test

        [TestMethod]
        public void IndexOf_Test()
        {
            int count = _unitUnderTest.Count;
            int dbl = count * 2;
            int[] arr = new int[dbl];
            var result = _unitUnderTest.IndexOf(4);

            Assert.AreEqual(result, 3);
        }

        #endregion // IndexOf_Test

        #region IndexOf_Stress_Test

        [TestMethod]
        public async Task IndexOf_Stress_Test()
        {
            var tasks = _unitUnderTestLarge
                .Select((v, i) => Task.Run(() =>
                {
                    int idx = _unitUnderTestLarge.IndexOf(v);
                    Assert.AreEqual(i, idx);
                }));

            await Task.WhenAll(tasks);
        }

        #endregion // IndexOf_Stress_Test
    }
}
