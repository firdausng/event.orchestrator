using CloudNative.CloudEvents;

namespace app.core.Data.CloudEvents;

public interface ICloudEventConsumerHandler
{
    Task<CloudEventStatus> HandleAsync(CloudEvent payload);
}

public record CloudEventStatus(bool IsSuccess, string Message, Exception? Exception = null);