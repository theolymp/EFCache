// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System.Data.Entity.Core.Metadata.Edm;

#endregion

namespace EFCache
{
    public sealed class BlacklistedQueriesRegistrar
    {
        public static readonly BlacklistedQueriesRegistrar Instance = new BlacklistedQueriesRegistrar();

        private readonly QueryRegistrar _blacklistedQueries = new QueryRegistrar();

        private BlacklistedQueriesRegistrar()
        {
        }

        public bool IsQueryBlacklisted(MetadataWorkspace workspace, string sql)
        {
            return _blacklistedQueries.ContainsQuery(workspace, sql);
        }

        public bool RemoveBlacklistedQuery(MetadataWorkspace workspace, string sql)
        {
            return _blacklistedQueries.RemoveQuery(workspace, sql);
        }

        public void AddBlacklistedQuery(MetadataWorkspace workspace, string sql)
        {
            _blacklistedQueries.AddQuery(workspace, sql);
        }
    }
}