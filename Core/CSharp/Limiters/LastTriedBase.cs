using System;
using System.Collections.Generic;

namespace Core.Limiters
{
    public class LastTriedBase
    {
        private int _NMillisecondsWindow;
        private int _MaxNRecentToAllowTryAgain;
        private LinkedList<long> _LastTries;
        public LastTriedBase(long millisecondsUTCTriedAt, int nMillisecondsWindow, int maxNRecentToAllowTryAgain) {
            _NMillisecondsWindow = nMillisecondsWindow;
            _MaxNRecentToAllowTryAgain = maxNRecentToAllowTryAgain;
            _LastTries = new LinkedList<long>();
            _LastTries.AddFirst(millisecondsUTCTriedAt);
        }
        public bool CanDoAgainNow(long millisecondsNowUTC, out int secondsToWait) {
            long recentTriesFromMillisecondsUTC = millisecondsNowUTC - _NMillisecondsWindow;
            secondsToWait = 0;
            int nRecent = 0;
            int index = _LastTries.Count - 1;
            long lastRecentTryMillisecondsUTC = 0;
            LinkedListNode<long> node = _LastTries.Last;
            while (index>=0) {
                long lastTryMillisecondsUTC = node.Value;
                if (lastTryMillisecondsUTC < recentTriesFromMillisecondsUTC) {
                    break;
                }
                nRecent++;
                lastRecentTryMillisecondsUTC = lastTryMillisecondsUTC;
                node = node.Previous;
                index--;
            }
            while (index >= 0) {
                _LastTries.RemoveFirst();
                index--;
            }
            if (nRecent >= _MaxNRecentToAllowTryAgain)
            {
                long millisecondsToWait = _NMillisecondsWindow - (millisecondsNowUTC - lastRecentTryMillisecondsUTC);
                long secondsToWaitInternal = (long)Math.Ceiling((double)millisecondsToWait / 1000d);
                if (secondsToWaitInternal > int.MaxValue)
                    secondsToWaitInternal = int.MaxValue;
                else if (secondsToWaitInternal < 0)
                    secondsToWaitInternal = 0;
                secondsToWait = (int)secondsToWaitInternal;
            }
            return secondsToWait<=0;
        }
        public void Tried(long millisecondsUTCTriedAt) {
            _LastTries.AddLast(millisecondsUTCTriedAt);
        }
    }
}
