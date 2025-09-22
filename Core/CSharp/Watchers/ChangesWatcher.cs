using Core.Cleanup;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.VirtualSockets
{
    public class ChangesWatcher<TKey>
    {
        private Dictionary<TKey, HashSet<Action<TKey>>> _MapKeyToChangeds = new Dictionary<TKey, HashSet<Action<TKey>>>();
        public CleanupHandle Watch(TKey key, Action<TKey> changed) {
            CleanupHandle cleanup = new CleanupHandle(() =>
            {
                lock (_MapKeyToChangeds)
                {
                    if (_MapKeyToChangeds.TryGetValue(key, out HashSet<Action<TKey>> watchers))
                    {
                        watchers.Remove(changed);
                        if (watchers.Count < 1)
                        {
                            _MapKeyToChangeds.Remove(key);
                        }
                    }
                }
            });
            lock (_MapKeyToChangeds) {

                if (!_MapKeyToChangeds.TryGetValue(key, out HashSet<Action<TKey>> watchers))
                {
                    _MapKeyToChangeds.Add(key, new HashSet<Action<TKey>> { changed });
                    return cleanup;
                }
                if(watchers.Contains(changed))
                    return cleanup;
                watchers.Add(changed);
            }
            return cleanup;
        }
        public void Changed_DoNotCallInsideLocksOnObjectOrOnDatabase(TKey key)
        { 
            Action<TKey>[] changeds;
            lock (_MapKeyToChangeds)
            {
                if (!_MapKeyToChangeds.TryGetValue(key, out HashSet<Action<TKey>>changedsHashSet))
                    return;
                changeds = changedsHashSet.ToArray();
            }
            foreach (Action<TKey> changed in changeds)
            {
                try
                {
                    changed(key);
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }
        }
    }
}
