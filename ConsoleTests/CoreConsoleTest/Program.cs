using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            #region CompleteWhenN

            Console.WriteLine("Complete When N");
            Task t4 = CompleteWhenN();
            while (!t4.IsCompleted)
            {
                Console.Write(".");
                Thread.Sleep(50);
            }

            #endregion // CompleteWhenN

            Console.ReadKey(true);
        }


        #region CompleteWhenN

        private static async Task CompleteWhenN()
        {
            var tasks = from i in Enumerable.Range(0, 20)
                        select SingleStepWithResultAsync(i);
                        //select SingleStepAsync(i);
            await tasks.WhenN(2);
            Console.WriteLine("COMPLETE");
        }

        #endregion // CompleteWhenN

        #region SingleStepAsync

        private static async Task<int> SingleStepWithResultAsync(int i)
        {
            await Task.Delay(i < 2 ? 500 : 2000);
            Console.Write($"{i}, ");
            return i;
        }

        private static async Task SingleStepAsync(int i)
        {
            await Task.Delay(i < 2 ? 500 : 2000);
            Console.Write($"{i}, ");
        }

        #endregion // SingleStepAsync
    }
}
