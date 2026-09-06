using System;
using System.Collections.Generic;
using System.Threading;

namespace Project.Processing
{
    public static class ConcurrencyControl
    {
        private static readonly object _listLock = new object();
        private static readonly List<ItemLock> _locks = new List<ItemLock>();

        public static IDisposable AcquireLock(int repoId, string itemKey)
        {
            return new ItemLock(repoId, itemKey);
        }

        private class ItemLock : IDisposable
        {
            private readonly Tuple<int, string> _name;
            private readonly object _lock;

            public ItemLock(int repoId, string itemKey)
            {
                _name = Tuple.Create(repoId, itemKey);
                lock (_listLock)
                {
                    var existing = _locks.Find(l => l._name.Equals(_name));
                    // Allow one lock instance per name
                    _lock = existing == null ? new object() : existing._lock;
                    _locks.Add(this);
                }
                Monitor.Enter(_lock);
            }

            public void Dispose()
            {
                if (Monitor.IsEntered(_lock)) //can be skipped if usages are from your code only
                    Monitor.Exit(_lock);

                lock (_listLock)
                {
                    _locks.Remove(this);
                }
            }
        }
    }
}