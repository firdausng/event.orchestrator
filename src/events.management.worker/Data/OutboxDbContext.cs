using events.management.core.Domains.Entities;
using Microsoft.EntityFrameworkCore;

namespace events.management.worker.Data;

public class OutboxDbContext: DbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {

    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}