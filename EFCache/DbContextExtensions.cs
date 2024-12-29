// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

#endregion

namespace EFCache
{
    public static class DbContextExtensions
    {
        public static CachingProviderServices GetCachingProviderServices(this DbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IObjectContextAdapter)context).ObjectContext.GetCachingProviderServices();
        }
    }
}