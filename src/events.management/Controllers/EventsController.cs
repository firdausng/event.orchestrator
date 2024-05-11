using System.Diagnostics;
using events.management.Commands;
using events.management.Models;
using Microsoft.AspNetCore.Mvc;

namespace events.management.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController: Controller
{
    private readonly CreateEventConfigurationCommand _createEventConfigurationCommand;

    public EventsController(CreateEventConfigurationCommand createEventConfigurationCommand)
    {
        _createEventConfigurationCommand = createEventConfigurationCommand;
    }
    [HttpPost("", Name = "PublishEvents")]
    public async Task<IActionResult> Post(List<CreateEventConfigurationRequest> eventConfigurationRequests)
    {
        Activity.Current?.SetTag("request.id", eventConfigurationRequests.First().Id);
        await _createEventConfigurationCommand.Handle(eventConfigurationRequests.First());
        return Ok(eventConfigurationRequests);
    }
}