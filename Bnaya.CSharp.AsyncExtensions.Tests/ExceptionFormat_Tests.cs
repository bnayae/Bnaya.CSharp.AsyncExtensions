using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Bnaya.CSharp.AsyncExtensions.Tests
{
    [TestClass]
    public class ExceptionFormat_Tests
    {
        #region FormattingException_HaveAllStackInOrder_Test

        [TestMethod]
        public async Task FormattingException_HaveAllStackInOrder_Test()
        {
            try
            {
                await Step1Async(10);
            }
            catch (Exception ex)
            {
                var formatted = ex.Format();
                int idx0 = formatted.IndexOf(nameof(FormattingException_HaveAllStackInOrder_Test));
                Assert.AreNotEqual(-1, idx0);
                int idx1 = formatted.IndexOf(nameof(Step1Async));
                Assert.AreNotEqual(-1, idx1);
                Assert.IsTrue(idx1 < idx0);
                int idx2 = formatted.IndexOf(nameof(Step2Async));
                Assert.AreNotEqual(-1, idx2);
                Assert.IsTrue(idx2 < idx1);
                int idx3 = formatted.IndexOf(nameof(OtherClass.Step3Async));
                Assert.AreNotEqual(-1, idx3);
                Assert.IsTrue(idx3 < idx2);
                int idx4 = formatted.IndexOf(nameof(OtherClass.Step4Async));
                Assert.AreNotEqual(-1, idx4);
                Assert.IsTrue(idx4 < idx3);
                int idx5 = formatted.IndexOf(nameof(OtherClass.Step5Async));
                Assert.AreNotEqual(-1, idx5);
                Assert.IsTrue(idx5 < idx4);

                // check duplication
                int idx4x = formatted.IndexOf(nameof(OtherClass.Step4Async), idx4+ 2);
                Assert.AreEqual(-1, idx4x);
            }

        }

        #endregion // FormattingException_HaveAllStackInOrder_Test

        #region FormattingException_WhenAllFlow_HaveAllStackInOrder_Test

        [TestMethod]
        public async Task FormattingException_WhenAllFlow_HaveAllStackInOrder_Test()
        {
            try
            {
                await StepAAsync(10);
            }
            catch (Exception ex)
            {
                var formatted = ex.Format();
                int idx0 = formatted.IndexOf(nameof(FormattingException_HaveAllStackInOrder_Test));
                Assert.AreNotEqual(-1, idx0);
                int idx1 = formatted.IndexOf(nameof(Step1Async));
                Assert.AreNotEqual(-1, idx1);
                Assert.IsTrue(idx1 > idx0);
                int idx2 = formatted.IndexOf(nameof(Step2Async));
                Assert.AreNotEqual(-1, idx2);
                Assert.IsTrue(idx2 > idx1);
                int idx3 = formatted.IndexOf(nameof(OtherClass.Step3Async));
                Assert.AreNotEqual(-1, idx3);
                Assert.IsTrue(idx3 > idx2);
                int idx4 = formatted.IndexOf(nameof(OtherClass.Step4Async));
                Assert.AreNotEqual(-1, idx4);
                Assert.IsTrue(idx4 > idx3);
                int idx5 = formatted.IndexOf(nameof(OtherClass.Step5Async));
                Assert.AreNotEqual(-1, idx5);
                Assert.IsTrue(idx5 > idx4);

                // check duplication
                int idx4x = formatted.IndexOf(nameof(OtherClass.Step4Async), idx4+ 2);
                Assert.AreEqual(-1, idx4x);
            }

        }

        #endregion // FormattingException_WhenAllFlow_HaveAllStackInOrder_Test

        #region Call-Chain

        #region Step1Async

        private static async Task Step1Async(int i)
        {
            await Task.Delay(1);
            var s = new string('*', i);
            await Step2Async(s);
        }

        #endregion // Step1Async

        #region Step2Async

        private static async Task Step2Async(string s)
        {
            try
            {
                await Task.Delay(1);
                await OtherClass.Step3Async(s);
            }
            catch (Exception ex)
            {
                throw new NullReferenceException("in between", ex);
            }
        }

        #endregion // Step2Async


        private class OtherClass
        {
            #region Step3Async

            public static async Task Step3Async(string s1)
            {
                await Task.Delay(1);
                await Step4Async(s1);
            }

            #endregion // Step3Async

            #region Step4Async

            public static async Task Step4Async(string s2)
            {
                try
                {
                    await Task.Delay(1);
                    await Step5Async(s2);
                }
                catch (Exception ex)
                {
                    throw new ArgumentOutOfRangeException("Range 1-1".PadRight(60, '0') , ex);
                }
            }

            #endregion // Step4Async

            #region Step5Async

            public static async Task Step5Async(string s3)
            {
                await Task.Delay(1);
                throw new FormatException($"Illegal {s3}");
            }

            #endregion // Step5Async
        }

        #endregion // Call-Chain

        #region Call-Chain (parallel)

        #region StepAAsync

        private static async Task StepAAsync(int j)
        {
            await Task.Delay(1);
            await StepBAsync(DateTime.Now.AddDays(j));
        }

        #endregion // StepAAsync

        #region StepBAsync

        private static async Task StepBAsync(DateTime dt)
        {
            var t1 = Task.Run(() => throw new ArgumentException("Other Error"));
            var t2 = Step1Async(dt.Second);
            await Task.WhenAll(t1, t2).ThrowAll();
        }

        #endregion // StepBAsync

        #endregion // Call-Chain (parallel)
    }
}
