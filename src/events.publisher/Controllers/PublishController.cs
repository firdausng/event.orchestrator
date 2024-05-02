using System.Diagnostics;
using events.publisher.Commands;
using events.publisher.Models;
using Microsoft.AspNetCore.Mvc;

namespace events.publisher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishController: Controller
{
    private readonly PublishEventListCommand _publishEventListCommand;

    public PublishController(PublishEventListCommand publishEventListCommand)
    {
        _publishEventListCommand = publishEventListCommand;
    }
    [HttpPost("", Name = "PublishEvents")]
    public async Task<IActionResult> Post(List<PublishEventRequest> publishEventRequests, CancellationToken cancellationToken)
    {
        Activity.Current?.SetTag("request.id", publishEventRequests.First().Id);
        await _publishEventListCommand.Handle(publishEventRequests.First());
        return Ok();
    }
}