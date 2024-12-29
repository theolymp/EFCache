// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;

#endregion

namespace EFCache
{
    internal class CachingCommandDefinition : DbCommandDefinition
    {
        private readonly CacheTransactionHandler _cacheTransactionHandler;
        private readonly CachingPolicy _cachingPolicy;
        private readonly DbCommandDefinition _commandDefintion;
        private readonly CommandTreeFacts _commandTreeFacts;

        public CachingCommandDefinition(DbCommandDefinition commandDefinition, CommandTreeFacts commandTreeFacts,
            CacheTransactionHandler cacheTransactionHandler, CachingPolicy cachingPolicy)
        {
            _commandDefintion = commandDefinition;
            _commandTreeFacts = commandTreeFacts;
            _cacheTransactionHandler = cacheTransactionHandler;
            _cachingPolicy = cachingPolicy;
        }

        public ReadOnlyCollection<EntitySetBase> AffectedEntitySets => _commandTreeFacts.AffectedEntitySets;

        public bool IsCacheable => _commandTreeFacts.IsQuery && !_commandTreeFacts.UsesNonDeterministicFunctions;

        public bool IsQuery => _commandTreeFacts.IsQuery;

        public override DbCommand CreateCommand()
        {
            return new CachingCommand(_commandDefintion.CreateCommand(), _commandTreeFacts, _cacheTransactionHandler,
                _cachingPolicy);
        }
    }
}