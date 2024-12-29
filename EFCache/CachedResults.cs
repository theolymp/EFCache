// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System;
using System.Collections.Generic;

#endregion

namespace EFCache
{
    [Serializable]
    internal class CachedResults
    {
        public CachedResults(ColumnMetadata[] tableMetadata, List<object[]> results, int recordsAffected)
        {
            TableMetadata = tableMetadata;
            Results = results;
            RecordsAffected = recordsAffected;
        }

        public int RecordsAffected { get; }

        public List<object[]> Results { get; }

        public ColumnMetadata[] TableMetadata { get; }
    }
}