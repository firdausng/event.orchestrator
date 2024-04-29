using app.core.Data.CloudEvents;
using CloudNative.CloudEvents;
using events.subscriber.Options;
using Microsoft.Extensions.Options;

namespace events.subscriber.Services;

public class CloudEventConsumerHandler : ICloudEventConsumerHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudEventConsumerHandler> _logger;
    private readonly CloudEventHandlerOptions _options;

    public CloudEventConsumerHandler(HttpClient httpClient, ILogger<CloudEventConsumerHandler> logger, IOptions<CloudEventHandlerOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }
    
    public async Task<CloudEventStatus> HandleAsync(CloudEvent payload)
    {
        using var content = JsonContent.Create(payload.Data);
        var req = new HttpRequestMessage();
        req.RequestUri = _options.Url;
        req.Content = content;
        req.Method = HttpMethod.Post;

        try
        {
            var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            return !response.IsSuccessStatusCode ? new CloudEventStatus(false, "Error sending webhook") : new CloudEventStatus(true, "Webhook sent successfully");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending request");
            return new CloudEventStatus(false, "Error sending webhook", e);
        }
    }
}
