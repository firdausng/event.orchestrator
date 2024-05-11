using events.management.Domains.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.management.Data.Configurations;

internal class EventConfigurationDbContextConfiguration: IEntityTypeConfiguration<EventConfiguration>
{
    public void Configure(EntityTypeBuilder<EventConfiguration> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value, 
                value => new EventConfigurationId(value)
                );
        
        builder
            .HasOne(e => e.Destination);
    }
}

internal class DestinationConfiguration: IEntityTypeConfiguration<Destination>
{
    public void Configure(EntityTypeBuilder<Destination> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value, 
                value => new DestinationId(value)
                );
        
        builder.Property(c => c.Type)
            .HasConversion(
                id => id.Value, 
                value => new DestinationType(value)
            );
        
        
        builder
            .HasDiscriminator<string>("DestinationDiscriminator")
            .HasValue<WebhookDestination>("WebhookDestination")
            .HasValue<GoogleDestination>("GoogleDestination");
    }
}
