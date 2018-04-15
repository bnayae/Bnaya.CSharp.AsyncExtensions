using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

// TODO: New Thread, Task.Factory.StartNew, Task.Run

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
                int idx4x = formatted.IndexOf(nameof(OtherClass.Step4Async), idx4 + 2);
                Assert.AreEqual(-1, idx4x);
            }

        }

        #endregion // FormattingException_HaveAllStackInOrder_Test

        #region FormattingException_HaveAllStackInOrder_WithLocation_Test

        [TestMethod]
        public async Task FormattingException_HaveAllStackInOrder_WithLocation_Test()
        {
            try
            {
                await Step1Async(10);
            }
            catch (Exception ex)
            {
                var formatted = ex.FormatWithLineNumber();
                int idx0 = formatted.IndexOf(nameof(FormattingException_HaveAllStackInOrder_WithLocation_Test));
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
                int idx4x = formatted.IndexOf(nameof(OtherClass.Step4Async), idx4 + 2);
                Assert.AreNotEqual(-1, idx4x, "includeLocation option don't remove duplication");

                // check location
                int idx6 = formatted.IndexOf(".cs");
                Assert.AreNotEqual(-1, idx6);
                int idx7 = formatted.IndexOf(" line ");
                Assert.AreNotEqual(-1, idx7);
            }

        }

        #endregion // FormattingException_HaveAllStackInOrder_WithLocation_Test

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


                int idxArrow1 = formatted.IndexOf("~ 1 ~>");
                int idxArrow2 = formatted.IndexOf("~ 2 ~>");
                Assert.IsTrue(idxArrow1 < idxArrow2);
                int idxA0 = formatted.IndexOf("# Throw (ArgumentException): Other Error");
                Assert.IsTrue(idxArrow1 < idxA0);
                Assert.IsTrue(idxA0 < idxArrow2);
                Assert.AreNotEqual(-1, idxA0);
                int idxB = formatted.IndexOf(nameof(StepBAsync));
                int idx1 = formatted.IndexOf(nameof(Step1Async));
                Assert.AreNotEqual(-1, idx1);
                Assert.IsTrue(idx1 > idxA0);
                int idx2 = formatted.IndexOf(nameof(Step2Async));
                Assert.AreNotEqual(-1, idx2);
                Assert.IsTrue(idx2 < idx1);


                // check duplication
                int idxStart = formatted.IndexOf("~ Start Task ~>");
                int idxA = formatted.IndexOf(nameof(StepAAsync));
                int idxTest = formatted.IndexOf(nameof(FormattingException_WhenAllFlow_HaveAllStackInOrder_Test));
                Assert.AreNotEqual(-1, idxStart);
                Assert.AreNotEqual(-1, idxA);
                Assert.AreNotEqual(-1, idxTest);
                Assert.IsTrue(idxStart < idxA && idxA < idxTest);
            }

        }

        #endregion // FormattingException_WhenAllFlow_HaveAllStackInOrder_Test

        #region FormattingException_ParallelForFlow_HaveAllStackInOrder_Test

        [TestMethod]
        public void FormattingException_ParallelForFlow_HaveAllStackInOrder_Test()
        {
            try
            {
                StepForAsync();
            }
            catch (Exception ex)
            {
                var formatted = ex.Format();
                int idx3 = formatted.IndexOf("~ 3 ~>");
                int idxStart = formatted.IndexOf("~ Start Task ~>");
                int idxFor = formatted.IndexOf("Parallel.For");
                Assert.AreNotEqual(-1, idx3);
                Assert.AreNotEqual(-1, idxStart);
                Assert.AreNotEqual(-1, idxFor);
                Assert.IsTrue(idx3 < idxStart&& idxStart < idxFor);
            }

        }

        #endregion // FormattingException_ParallelForFlow_HaveAllStackInOrder_Test

        #region FormattingException_Linq_HaveAllStackInOrder_Test

        [TestMethod]
        public async Task FormattingException_Linq_HaveAllStackInOrder_Test()
        {
            try
            {
                await StepLinqAsync().ThrowAll();
            }
            catch (Exception ex)
            {
                var formatted = ex.Format();
                int idx3 = formatted.IndexOf("~ 2 ~>");
                int idxStart = formatted.IndexOf("~ Start ~>");
                int idxTest = formatted.IndexOf(nameof(FormattingException_Linq_HaveAllStackInOrder_Test));
                Assert.AreNotEqual(-1, idx3);
                Assert.AreNotEqual(-1, idxStart);
                Assert.AreNotEqual(-1, idxTest);
                Assert.IsTrue(idx3 < idxStart&& idxStart < idxTest);
            }

        }

        #endregion // FormattingException_Linq_HaveAllStackInOrder_Test


        #region FormattingException_DashFormat_HaveAllStackInOrder_Test

        [TestMethod]
        public async Task FormattingException_DashFormat_HaveAllStackInOrder_Test()
        {
            try
            {
                await Step1Async(10);
            }
            catch (Exception ex)
            {
                var formatted = ex.Format(ErrorFormattingOption.FormatDuplication);
                var lines = formatted.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int replaced = lines.Where(l => l.StartsWith("-----.------.")).Count();
                Assert.AreEqual(3, replaced);
            }

        }

        #endregion // FormattingException_DashFormat_HaveAllStackInOrder_Test

        #region FormattingException_WhenAllFlow_DashFormat_Test

        [TestMethod]
        public async Task FormattingException_WhenAllFlow_DashFormat_Test()
        {
            try
            {
                await StepAAsync(10);
            }
            catch (Exception ex)
            {
                var formatted = ex.Format(ErrorFormattingOption.FormatDuplication);

                var lines = formatted.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int replaced = lines.Where(l => l.StartsWith("-----.------.")).Count();
                Assert.AreEqual(3, replaced);

                int idxArrow1 = formatted.IndexOf("~ 1 ~>");
                int idxArrow2 = formatted.IndexOf("~ 2 ~>");
                Assert.IsTrue(idxArrow1 < idxArrow2);
                int idxA0 = formatted.IndexOf("# Throw (ArgumentException): Other Error");
                Assert.IsTrue(idxArrow1 < idxA0);
                Assert.IsTrue(idxA0 < idxArrow2);
                Assert.AreNotEqual(-1, idxA0);
                int idxB = formatted.IndexOf(nameof(StepBAsync));
                int idx1 = formatted.IndexOf(nameof(Step1Async));
                Assert.AreNotEqual(-1, idx1);
                Assert.IsTrue(idx1 > idxA0);
                int idx2 = formatted.IndexOf(nameof(Step2Async));
                Assert.AreNotEqual(-1, idx2);
                Assert.IsTrue(idx2 < idx1);

            }

        }

        #endregion // FormattingException_WhenAllFlow_DashFormat_Test


        #region HideDuplicatePaths_Shorter_Test

        [TestMethod]
        public void HideDuplicatePaths_Shorter_Test()
        {
            string a = "aaa1.bbb2.ccc3.eee4";
            string b = "aaa1.bbb2.ddd3.ff4";
            var (c, _) = BnayaErrorHandlinglExtensions.HideDuplicatePaths(a, b);

            Assert.AreEqual("----.----.ddd3.ff4", c);
        }

        #endregion // HideDuplicatePaths_Shorter_Test

        #region HideDuplicatePaths_Longer_Test

        [TestMethod]
        public void HideDuplicatePaths_Longer_Test()
        {
            string a = "aaa1.bbb2.ccc3.eee4";
            string b = "aaa1.bbb2.ddd3.ffff4";
            var (c, count) = BnayaErrorHandlinglExtensions.HideDuplicatePaths(a, b);

            Assert.AreEqual("----.----.ddd3.ffff4", c);
            Assert.AreEqual(2, count);
        }

        #endregion // HideDuplicatePaths_Longer_Test

        #region HideDuplicatePaths_StartKeepTab_Test

        [TestMethod]
        public void HideDuplicatePaths_StartKeepTab_Test()
        {
            string a = "\taaa1.bbb2.ccc3.eee4";
            string b = "\taaa1.bbb2.ddd3.ff4";
            var (c, _) = BnayaErrorHandlinglExtensions.HideDuplicatePaths(a, b);

            Assert.AreEqual("\t----.----.ddd3.ff4", c);
        }

        #endregion // HideDuplicatePaths_StartKeepTab_Test

        #region HideDuplicatePaths_KeepTab_Test

        [TestMethod]
        public void HideDuplicatePaths_KeepTab_Test()
        {
            string a = "\taaa1.\tbbb2.ccc3.eee4";
            string b = "\taaa1.\tbbb2.\tddd3.ff4";
            var (c, _) = BnayaErrorHandlinglExtensions.HideDuplicatePaths(a, b);

            Assert.AreEqual("\t----.\t----.\tddd3.ff4", c);
        }

        #endregion // HideDuplicatePaths_KeepTab_Test


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


        private static class OtherClass
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

        private static Task StepBAsync(DateTime dt)
        {
            var t1 = Task.Run(() => throw new ArgumentException("Other Error"));
            var t2 = Step1Async(dt.Second);
            return Task.WhenAll(t1, t2).ThrowAll();
        }

        #endregion // StepBAsync

        #endregion // Call-Chain (parallel)

        #region Call-Chain (Loop)

        #region StepForAsync

        private static void StepForAsync()
        {
            Parallel.For(0, 3, i => Step1Async(i).Wait());
        }

        #endregion // StepForAsync

        #region StepLinqAsync

        private static Task StepLinqAsync()
        {
            var query = Enumerable.Range(0, 2).Select(Step1Async);
            return Task.WhenAll(query);
        }

        #endregion // StepLinqAsync

        #endregion // Call-Chain (Loop)
    }
}
