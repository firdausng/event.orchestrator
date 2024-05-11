using events.management.Data.Interceptors;
using events.management.Domains.Entities;
using Microsoft.EntityFrameworkCore;

namespace events.management.Data;

public class EventsManagementDbContext: DbContext
{
    public DbSet<EventConfiguration> EventConfigurations { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
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