// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;

namespace Tama.Core;

public sealed class ReadOnlyTamaDbContext : TamaDbContext
{
    public ReadOnlyTamaDbContext(DbContextOptions<TamaDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }
}
