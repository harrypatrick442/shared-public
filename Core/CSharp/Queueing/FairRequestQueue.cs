using System;
using System.Collections.Generic;

namespace Core.Queueing
{
    public class FairRequestQueue
    {
        private int _MaxNEntries;
        private Queue _Queue;
        public delegate void DelegateHandleRequestQueueEntry(IRequestQueueEntry requestQueueEntry);
        public FairRequestQueue(int maxNEntriesToProcessAtOnce, int maxNEntries) {
            _MaxNEntries = maxNEntries;
            _Queue = new Queue(maxNEntriesToProcessAtOnce, PickNextEntryToProcess);
        }
        public void AddHandler(Type entryType, DelegateHandleRequestQueueEntry handleRequestQueueEntry) {
            _Queue.AddHandler(entryType, (entry)=>
                handleRequestQueueEntry((IRequestQueueEntry)entry)
            );
        }
        public void Add(IRequestQueueEntry requestQueueEntry) {
            CheckNotTooManyQueued();
            _Queue.Add(requestQueueEntry);
        }
        private object PickNextEntryToProcess(List<object> entries) {
            if (entries.Count < 1) return null;
            object entry = entries[0];
            PlaceRemainingEntriesForSameIpToTheBackOfTheQueue(entries, (IRequestQueueEntry)entry);
            return entry;
        }
        private void PlaceRemainingEntriesForSameIpToTheBackOfTheQueue(List<object> entries, IRequestQueueEntry entry) {
            string ip = entry.Ip;
            List<object> toPlaceToBacks = new List<object>();
            foreach (object remainingEntry in entries) {
                IRequestQueueEntry remainingRequestQueueEntry = (IRequestQueueEntry)remainingEntry;
                if (remainingRequestQueueEntry.Ip == ip) {
                    toPlaceToBacks.Add(remainingRequestQueueEntry);
                }
            }
            foreach (object toPlaceToBack in toPlaceToBacks)
            {
                entries.Remove(toPlaceToBack);
                entries.Add(toPlaceToBack);
            }
        }
        private void CheckNotTooManyQueued() {
            int nEntries = _Queue.NEntries;
            if (nEntries > _MaxNEntries) {
                throw new QueueBackedUpException(nameof(FairRequestQueue), nEntries);
            }
        }
    }
}
