// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

#endregion

namespace EFCache
{
    internal class QueryRegistrar
    {
        private readonly ConcurrentDictionary<MetadataWorkspace, HashSet<string>> _queries =
            new ConcurrentDictionary<MetadataWorkspace, HashSet<string>>();

        public bool ContainsQuery(MetadataWorkspace workspace, string sql)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));

            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            if (_queries.TryGetValue(workspace, out var queries))
                lock (queries)
                {
                    return queries.Contains(sql);
                }

            return false;
        }

        public bool RemoveQuery(MetadataWorkspace workspace, string sql)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));

            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            if (_queries.TryGetValue(workspace, out var queries))
                lock (queries)
                {
                    return queries.Remove(sql);
                }

            return false;
        }

        public void AddQuery(MetadataWorkspace workspace, string sql)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));

            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            var queries = _queries.GetOrAdd(workspace, new HashSet<string>());
            lock (queries)
            {
                queries.Add(sql);
            }
        }
    }
}