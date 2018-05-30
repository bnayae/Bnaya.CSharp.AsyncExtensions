using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Bnaya.CSharp.CollectionExtensions.Benchmarks
{
    static class Program
    {
        static void Main()
        {            
            var summary = BenchmarkRunner.Run<ConcurrentListBenchmarks>();
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine(summary);
            Console.ReadLine();
        }
    }
}
