using System;

namespace Mane.Unity
{
    public class WaitForSecondsRealtimeWhile : WaitForSecondsUntilBase
    {
        public WaitForSecondsRealtimeWhile(Func<bool> predicate, float waitSeconds, bool checkPredicateFirst = false)
            : base(predicate, waitSeconds, checkPredicateFirst) { }

        public override bool keepWaiting => IsKeepWaiting(false, false);
    }
}