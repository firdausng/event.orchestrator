using events.configuration.Domains.Entities;
using Microsoft.EntityFrameworkCore;

namespace events.configuration.Data;

public class EventConfigurationDbContext: DbContext
{
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public EventConfigurationDbContext(DbContextOptions<EventConfigurationDbContext> options) : base(options)
    {

    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventConfigurationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}