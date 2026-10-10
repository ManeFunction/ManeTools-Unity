using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mane.Unity.Tests
{
    public class CoroutineExtensionsTests
    {
        [Test]
        public void ToCoroutine_Completed_ReportsResult()
        {
            (bool ok, int value) = Run(Task.FromResult(42));

            Assert.IsTrue(ok);
            Assert.AreEqual(42, value);
        }

        [Test]
        public void ToCoroutine_Faulted_ReportsFailureWithoutThrowing()
        {
            (bool ok, int value) = Run(Task.FromException<int>(new InvalidOperationException()));

            Assert.IsFalse(ok);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void ToCoroutine_Canceled_ReportsFailureWithoutThrowing()
        {
            (bool ok, int value) = Run(Task.FromCanceled<int>(new CancellationToken(true)));

            Assert.IsFalse(ok);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void ToCoroutine_Pending_YieldsUntilCompleted()
        {
            TaskCompletionSource<int> source = new();
            int calls = 0;
            IEnumerator routine = source.Task.ToCoroutine((_, _) => calls++);

            Assert.IsTrue(routine.MoveNext());
            Assert.IsTrue(routine.MoveNext());
            Assert.AreEqual(0, calls);

            source.SetResult(7);
            Assert.IsFalse(routine.MoveNext());
            Assert.AreEqual(1, calls);
        }

        private static (bool ok, int value) Run(Task<int> task)
        {
            bool ok = true;
            int value = -1;
            IEnumerator routine = task.ToCoroutine((o, v) =>
            {
                ok = o;
                value = v;
            });

            while (routine.MoveNext()) { }

            return (ok, value);
        }
    }
}
