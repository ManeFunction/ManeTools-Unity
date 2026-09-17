using System;

namespace Mane.Unity
{
    /// <summary>
    /// Yields while the predicate is true, checking every <c>waitSeconds</c> of unscaled time.
    /// </summary>
    public class WaitForSecondsRealtimeWhile : WaitForSecondsUntilBase
    {
        /// <summary>
        /// Creates an unscaled-time wait-while instruction.
        /// </summary>
        /// <param name="predicate">Continue waiting while this returns true.</param>
        /// <param name="waitSeconds">Unscaled seconds between checks.</param>
        /// <param name="checkPredicateFirst">Also check once before the first wait.</param>
        public WaitForSecondsRealtimeWhile(Func<bool> predicate, float waitSeconds, bool checkPredicateFirst = false)
            : base(predicate, waitSeconds, checkPredicateFirst) { }

        /// <summary>
        /// True while the predicate is still true.
        /// </summary>
        public override bool keepWaiting => IsKeepWaiting(false, false);
    }
}
