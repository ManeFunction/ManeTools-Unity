using System;

namespace Mane.Unity
{
    /// <summary>
    /// Yields until the predicate is true, checking every <c>waitSeconds</c> of unscaled time.
    /// </summary>
    public class WaitForSecondsRealtimeUntil : WaitForSecondsUntilBase
    {
        /// <summary>
        /// Creates an unscaled-time wait-until instruction.
        /// </summary>
        /// <param name="predicate">Continue waiting until this returns true.</param>
        /// <param name="waitSeconds">Unscaled seconds between checks.</param>
        /// <param name="checkPredicateFirst">Also check once before the first wait.</param>
        public WaitForSecondsRealtimeUntil(Func<bool> predicate, float waitSeconds, bool checkPredicateFirst = false)
            : base(predicate, waitSeconds, checkPredicateFirst) { }

        /// <summary>
        /// True while the predicate is still false.
        /// </summary>
        public override bool keepWaiting => IsKeepWaiting(true, true);
    }
}
