// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tama.Core;

/// <summary>
/// Design-time DbContext factory for EF Core to use when creating migrations
/// </summary>
public class TamaDbContextFactory : IDesignTimeDbContextFactory<TamaDbContext>
{
    public TamaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TamaDbContext>()
            .UseSqlite(
                "Data Source=:memory:",
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            )
            .Options;

        return new TamaDbContext(options);
    }
}
