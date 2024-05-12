using events.management.worker.Commands;
using events.management.worker.Data;
using Microsoft.EntityFrameworkCore;

namespace events.management.worker.Workers;

public class OutboxWorker: BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
                var messagePublisher = scope.ServiceProvider.GetRequiredService<PublishConfigurationEventCommand>();
            
                // Get unprocessed messages
                var messages = await dbContext.OutboxMessages.Where(message => !message.Published).ToListAsync(stoppingToken);

                foreach (var msg in messages)
                {
                    await messagePublisher.Handle(msg);
                    msg.Published = true;
                }

                if (messages.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                
            }
        
            // Wait before checking for unprocessed messages again
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}