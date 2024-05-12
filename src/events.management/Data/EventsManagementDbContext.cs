using events.management.core.Domains.Entities;
using events.management.Data.Interceptors;
using events.management.Domains.Entities;
using Microsoft.EntityFrameworkCore;

namespace events.management.Data;

public class EventsManagementDbContext: DbContext
{
    public DbSet<EventConfiguration> EventConfigurations => Set<EventConfiguration>();
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public EventsManagementDbContext(DbContextOptions<EventsManagementDbContext> options) : base(options)
    {

    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new OutboxInterceptor());
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}