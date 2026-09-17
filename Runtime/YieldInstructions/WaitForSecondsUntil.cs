using System;

namespace Mane.Unity
{
    /// <summary>
    /// Yields until the predicate is true, checking every <c>waitSeconds</c> of scaled time.
    /// </summary>
    public class WaitForSecondsUntil : WaitForSecondsUntilBase
    {
        /// <summary>
        /// Creates a scaled-time wait-until instruction.
        /// </summary>
        /// <param name="predicate">Continue waiting until this returns true.</param>
        /// <param name="waitSeconds">Seconds between checks.</param>
        /// <param name="checkPredicateFirst">Also check once before the first wait.</param>
        public WaitForSecondsUntil(Func<bool> predicate, float waitSeconds, bool checkPredicateFirst = false)
            : base(predicate, waitSeconds, checkPredicateFirst) { }

        /// <summary>
        /// True while the predicate is still false.
        /// </summary>
        public override bool keepWaiting => IsKeepWaiting(true, false);
    }
}
