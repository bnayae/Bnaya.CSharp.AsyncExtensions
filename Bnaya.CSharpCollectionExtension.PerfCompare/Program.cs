using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bnaya.CSharpCollectionExtension.PerfCompare
{
    static class Program
    {
        private const int STRESS_LEVEL = 10_000_000;
        private static List<int> _sync = new List<int>();
        private static TapList<int> _tap = new TapList<int>();
        private static TdfList<int> _tdf = new TdfList<int>();
        private static LockedList<int> _locked = new LockedList<int>();

        static async Task Main()
        {
            (string Title, ConsoleColor Color, Func<int, Task> Action)[] scenarios =
                {
                    ("Sync  ", ConsoleColor.Gray, i => {_sync.Add(i); return Task.CompletedTask; }),
                    ("Locked", ConsoleColor.White, i => {_locked.Add(i); return Task.CompletedTask; }),
                    ("TAP   ", ConsoleColor.Yellow, async i => await _tap.AddAsync(i)),
                    ("TDF   ", ConsoleColor.Green, _tdf.AddAsync),
                };

            Console.WriteLine("warm-up");
            foreach (var scenario in scenarios)
            {
                await scenario.Action(-1).ConfigureAwait(false);
            }

            await Task.Delay(500);
            Console.WriteLine("Start");
            for (int i = 0; i < 5; i++)
            {
                await Benchmark(scenarios);
            }

            Console.ReadKey();
        }

        private static async Task Benchmark((string Title, ConsoleColor Color, Func<int, Task> Action)[] scenarios)
        {
            foreach (var scenario in scenarios)
            {
                var sw = Stopwatch.StartNew();
                var tasks = Enumerable.Range(0, STRESS_LEVEL)
                                .Select(i => scenario.Action(i));
                await Task.WhenAll(tasks).ConfigureAwait(false);
                sw.Stop();
                Console.ForegroundColor = scenario.Color;
                Console.WriteLine($"{scenario.Title}: \t{sw.Elapsed:g}");
                Console.ResetColor();
            }
            Console.WriteLine("-------------------------------");
        }
    }
}
