using events.configuration.Domains.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events.configuration.Data.Configurations;



internal class DestinationConfiguration: IEntityTypeConfiguration<Configuration >
{
    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value, 
                value => new ConfigurationId(value)
                );
        
        builder
            .HasDiscriminator<string>("ConfigurationDiscriminator")
            .HasValue<WebhookConfiguration>("WebhookConfiguration");
    }
}
