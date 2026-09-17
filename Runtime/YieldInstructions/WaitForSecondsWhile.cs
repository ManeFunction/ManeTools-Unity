using System;

namespace Mane.Unity
{
    /// <summary>
    /// Yields while the predicate is true, checking every <c>waitSeconds</c> of scaled time.
    /// </summary>
    public class WaitForSecondsWhile : WaitForSecondsUntilBase
    {
        /// <summary>
        /// Creates a scaled-time wait-while instruction.
        /// </summary>
        /// <param name="predicate">Continue waiting while this returns true.</param>
        /// <param name="waitSeconds">Seconds between checks.</param>
        /// <param name="checkPredicateFirst">Also check once before the first wait.</param>
        public WaitForSecondsWhile(Func<bool> predicate, float waitSeconds, bool checkPredicateFirst = false)
            : base(predicate, waitSeconds, checkPredicateFirst) { }

        /// <summary>
        /// True while the predicate is still true.
        /// </summary>
        public override bool keepWaiting => IsKeepWaiting(false, false);
    }
}
