// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Tama.Core.Dtos;
using static Tama.Core.Features.Result<Tama.Core.Features.Queries.Entries.ViewEntriesResponse>;

namespace Tama.Core.Features.Queries.Entries;

public sealed class ViewEntriesHandler(ReadOnlyTamaDbContext db)
    : IQueryHandler<ViewEntriesQuery, Result<ViewEntriesResponse>>
{
    private const int PageSize = 12;

    /// <inheritdoc />
    public async Task<Result<ViewEntriesResponse>> HandleAsync(ViewEntriesQuery query)
    {
        var bracket = await db
            .Brackets.Include(b => b.Entries)
            .Where(b => b.Id == query.BracketId)
            .Select(b => new { b.Title, b.Entries })
            .FirstOrDefaultAsync();

        if (bracket is null)
            return Success(new ViewEntriesResponse("Unknown Bracket", [], 0, 1));

        return Success(
            new ViewEntriesResponse(
                bracket.Title,
                bracket
                    .Entries.OrderBy(e => e.Title)
                    .Skip(query.Page * PageSize)
                    .Take(PageSize)
                    .Select(EntryDto.FromEntry)
                    .ToList(),
                query.Page,
                (int)Math.Ceiling(bracket.Entries.Count / (double)PageSize)
            )
        );
    }
}
