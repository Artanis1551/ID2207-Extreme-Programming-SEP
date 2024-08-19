using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI;
using WebAPI.DataModels;
using WebAPI.EnumTypes;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly SEPRepository repository;

    public EventController(SEPRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("PostEventApplication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "CustomerService")]
    public IActionResult PostEventApplication([FromBody] EventApplication createEventApplication)
    {
            EventApplication eventApplication = createEventApplication;
        eventApplication.Assignee = UserRoles.SeniorCustomerService;
        eventApplication.Status = EventApplicationState.Pending;
        if (repository.AddEventApplication(eventApplication))
        {
            return Ok();  // OK = 200;
        }
        return BadRequest("Event application with this record number already exists");
    }

    [HttpGet("GetEventApplications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public List<EventApplication> GetEventApplications()
    {
        List<EventApplication> result = new();
        foreach (var role in Enum.GetValues(typeof(UserRoles))) 
        {
            if (User.IsInRole(role.ToString())) 
            {
                result.AddRange(repository.GetEventApplicationsForRole((UserRoles)role, EventApplicationState.Pending).Select(x => x).ToList());
            }
        }
        return result;
    }

    [HttpPost("ApproveEventApplication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ApproveEventApplication([FromBody] IDJSON id)
    {
        EventApplication? eventApplication = repository.GetEventApplication(id.Id);
        if (eventApplication != null)
        {
            if (!User.IsInRole(eventApplication.Assignee.ToString()))
            {
                return BadRequest("Not authorized to approve this event application");
            }

            switch (eventApplication.Assignee)
            {
                case UserRoles.SeniorCustomerService:
                    eventApplication.Assignee = UserRoles.FinancialManager;
                    break;

                case UserRoles.FinancialManager:
                    eventApplication.Assignee = UserRoles.AdministrationManager;
                    break;

                case UserRoles.AdministrationManager:
                    if (eventApplication.Status == EventApplicationState.Pending)
                    {
                        eventApplication.Status = EventApplicationState.Approved;
                    }
                    else
                    {
                        return BadRequest("Event Application already closed");
                    }
                    break;
            }
            repository.UpdateEventApplication(eventApplication);
            return Ok("Event approved");
        }
        return BadRequest("Event not found");
    }

    [HttpPost("RejectEventApplication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RejectEventApplication([FromBody] IDJSON id)
    {
        EventApplication? eventApplication = repository.GetEventApplication(id.Id);
        if (eventApplication != null)
        {
            if (!User.IsInRole(eventApplication.Assignee.ToString()))
            {
                return BadRequest("Not authorized to reject this event application");
            }

            eventApplication.Status = EventApplicationState.Rejected;
            repository.UpdateEventApplication(eventApplication);
            return Ok("Event rejected");
        }
        return BadRequest("Event not found");
    }
}
