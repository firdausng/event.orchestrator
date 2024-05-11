using System.Text.Json;
using events.management.Domains.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace events.management.Data.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not { } context)
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        
        var outboxData =
            // Iterate over the entries in the change tracker
            context
                .ChangeTracker
                .Entries<Entity>()
                .Select(entry => entry)
                .SelectMany(entity => new List<EntityEntry<Entity>> {entity})
                .Select(d => new OutboxMessage
                {
                    Content = d.Entity.Serialize(),
                    CreatedAt = DateTime.UtcNow,
                    Published = false,
                    EntryState = d.State.ToString(),
                    ClrType = d.Metadata.ClrType.FullName
                })
                .ToList();

        if (outboxData.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxData);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}