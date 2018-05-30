using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Bnaya.CSharp.CollectionExtensions.Benchmarks
{
    [ClrJob] //, RyuJitX64Job] //, LegacyJitX64Job]
    //[CoreJob]
    //[MonoJob]
    [MemoryDiagnoser]
    [RankColumn, MeanColumn, CategoriesColumn] //, MinColumn, MaxColumn]
    //[SimpleJob(RunStrategy.Throughput, launchCount: 5, warmupCount: 5, targetCount: 40, id: "Throughput")]
    //[SimpleJob(RunStrategy.Monitoring, launchCount: 5, warmupCount: 5, targetCount: 40, id: "Monitoring")]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class ConcurrentListBenchmarks
    {
        [Params(200, 1_000, 10_000)]
        public int STRESS_LEVEL = 200;

        private readonly object _gate = new object();
        private ConcurrentImmutableList<int> _concurrent;
        private List<int> _sync;
        private TapList<int> _tap;
        private TdfList<int> _tdf;
        private LockedList<int> _locked;

        #region Setup

        [GlobalSetup]
        public void GlobalSetup()
        {
            //ADDITIONS = EXPECTED.Skip(START_COUNT).ToArray();
        }
        [IterationSetup]
        public void IterationSetup()
        {
            var initData = Enumerable.Range(-500, 500).ToArray();
            _concurrent = new ConcurrentImmutableList<int>(initData);
            _sync = new List<int>(initData);
            _tap = new TapList<int>(initData);
            _tdf = new TdfList<int>(initData);
            _locked = new LockedList<int>(initData);
        }

        #endregion // Setup

        #region Add_Concurrent

        [Benchmark(Baseline = false), BenchmarkCategory("Add")]
        public Task Add_Concurrent()
        {
            var tasks = Enumerable.Range(0, STRESS_LEVEL)
                            .Select(i => Task.Run(() => _concurrent.Add(i)));
            return Task.WhenAll(tasks);
        }

        #endregion // Add_Concurrent

        #region Add_Tap

        [Benchmark(Baseline = false), BenchmarkCategory("Add")]
        public Task Add_Tap()
        {
            var tasks = Enumerable.Range(0, STRESS_LEVEL)
                            .Select(i => Task.Run(async () => await _tap.AddAsync(i)));
            return Task.WhenAll(tasks);
        }

        #endregion // Add_Tap

        #region Add_Tdf

        [Benchmark(Baseline = false), BenchmarkCategory("Add")]
        public Task Add_Tdf()
        {
            var tasks = Enumerable.Range(0, STRESS_LEVEL)
                            .Select(i => Task.Run(async () => await _tdf.AddAsync(i)));
            return Task.WhenAll(tasks);
        }

        #endregion // Add_Tdf

        #region Add_Lock

        [Benchmark(Baseline = false), BenchmarkCategory("Add")]
        public Task Add_Lock()
        {
            var tasks = Enumerable.Range(0, STRESS_LEVEL)
                            .Select(i => Task.Run(() => _locked.Add(i)));
            return Task.WhenAll(tasks);
        }

        #endregion // Add_Lock

        #region Add_Sync

        [Benchmark(Baseline = true), BenchmarkCategory("Add")]
        public Task Add_Sync()
        {
            var tasks = Enumerable.Range(0, STRESS_LEVEL)
                        .Select(i => Task.Run(() =>
                        {
                            lock (_gate)
                            {
                                _sync.Add(i);
                            }
                        }));
            return Task.WhenAll(tasks);
            
        }

        #endregion // Add_Sync

    }
}
