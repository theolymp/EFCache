// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace EFCache
{
    public class InMemoryCache : ICache
    {
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

        private readonly Dictionary<string, HashSet<string>>
            _entitySetToKey = new Dictionary<string, HashSet<string>>();

        public int Count => _cache.Count;

        public bool GetItem(string key, out object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            value = null;

            lock (_cache)
            {
                var now = DateTimeOffset.Now;

                if (_cache.TryGetValue(key, out var entry))
                {
                    if (EntryExpired(entry, now))
                    {
                        InvalidateItem(key);
                    }
                    else
                    {
                        entry.LastAccess = now;
                        value = entry.Value;
                        return true;
                    }
                }
            }

            return false;
        }

        public void InvalidateItem(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    _cache.Remove(key);

                    foreach (var set in entry.EntitySets)
                    {
                        if (_entitySetToKey.TryGetValue(set, out var keys)) keys.Remove(key);
                    }
                }
            }
        }

        public void InvalidateSets(IEnumerable<string> entitySets)
        {
            if (entitySets == null) throw new ArgumentNullException(nameof(entitySets));

            lock (_cache)
            {
                var itemsToInvalidate = new HashSet<string>();

                foreach (var entitySet in entitySets)
                {
                    if (_entitySetToKey.TryGetValue(entitySet, out var keys))
                    {
                        itemsToInvalidate.UnionWith(keys);

                        _entitySetToKey.Remove(entitySet);
                    }
                }

                foreach (var key in itemsToInvalidate) InvalidateItem(key);
            }
        }

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets,
            TimeSpan slidingExpiration, DateTimeOffset absoluteExpiration)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (dependentEntitySets == null) throw new ArgumentNullException(nameof(dependentEntitySets));

            lock (_cache)
            {
                var entitySets = dependentEntitySets.ToArray();

                _cache[key] = new CacheEntry(value, entitySets, slidingExpiration, absoluteExpiration);

                foreach (var entitySet in entitySets)
                {
                    if (!_entitySetToKey.TryGetValue(entitySet, out var keys))
                    {
                        keys = new HashSet<string>();
                        _entitySetToKey[entitySet] = keys;
                    }

                    keys.Add(key);
                }
            }
        }

        private static bool EntryExpired(CacheEntry entry, DateTimeOffset now)
        {
            return entry.AbsoluteExpiration < now || now - entry.LastAccess > entry.SlidingExpiration;
        }

        public void Purge()
        {
            Purge(false);
        }

        public void Purge(bool removeUnexpiredItems)
        {
            lock (_cache)
            {
                var now = DateTimeOffset.Now;
                var itemsToRemove = new HashSet<string>();

                foreach (var item in _cache)
                    if (removeUnexpiredItems || EntryExpired(item.Value, now))
                        itemsToRemove.Add(item.Key);

                foreach (var key in itemsToRemove) InvalidateItem(key);
            }
        }

        private class CacheEntry
        {
            public CacheEntry(object value, string[] entitySets, TimeSpan slidingExpiration,
                DateTimeOffset absoluteExpiration)
            {
                Value = value;
                EntitySets = entitySets;
                SlidingExpiration = slidingExpiration;
                AbsoluteExpiration = absoluteExpiration;
                LastAccess = DateTimeOffset.Now;
            }

            public DateTimeOffset AbsoluteExpiration { get; }

            public string[] EntitySets { get; }

            public DateTimeOffset LastAccess { get; set; }

            public TimeSpan SlidingExpiration { get; }

            public object Value { get; }
        }
    }
}