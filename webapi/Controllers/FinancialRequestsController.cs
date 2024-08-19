using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI;
using WebAPI.DataModels;
using WebAPI.EnumTypes;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class FinancialRequestsController : ControllerBase
{
    private readonly SEPRepository repository;

    public FinancialRequestsController(SEPRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("PostFinancialRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "ProductionManager, ServicesManager, AdministrationManager")]
    public IActionResult PostTask([FromBody] CreateFinancialRequestDTO createTask)
    {
        FinancialRequest request = new()
        {
            Amount = createTask.Amount,
            Reason = createTask.Reason,
            Department = createTask.Department,
            Reference = createTask.Reference,
            Status = "InProgress",
            CreatedBy = User.Identity.Name,
        };
           
        if (repository.AddFinancialRequest(request))
        {
            return Ok();  // OK = 200;
        }
        return BadRequest("Could not create task");
    }

    [HttpGet("GetFinancialRequests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "AdministrationManager, ServicesManager, ProductionManager, FinancialManager")]
    public List<FinancialRequest> GetFinancialRequests()
    {
        List<FinancialRequest> result = new();
        result.AddRange(repository.GetFinancialRequests());
        if (User.IsInRole(UserRoles.FinancialManager.ToString()))
        {
            result = result.Where(x => x.Status == "InProgress").ToList();
        }
        else
        {
            result = result.Where(x => x.CreatedBy == User.Identity.Name).ToList();
        }
        return result;
    }

    [HttpPost("ApproveFinancialRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "FinancialManager")]
    public IActionResult ApproveFinancialRequest([FromBody] IDJSON id)
    {
        FinancialRequest? request = repository.GetFinancialRequest(id.Id);
        if (request != null)
        {
            if (request.Status == "InProgress")
            {
                request.Status = "Approved";
            }
            else
            {
                return BadRequest("Recruitment Request already closed");
            }
            repository.UpdateFinancialRequest(request);
            return Ok("Task Completed");
        }
        return BadRequest("Task not found");
    }

    [HttpPost("RejectFinancialRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "FinancialManager")]
    public IActionResult RejectRecruitmentRequest([FromBody] IDJSON id)
    {
        FinancialRequest? request = repository.GetFinancialRequest(id.Id);
        if (request != null)
        {
            if (request.Status != "InProgress")
            {
                return BadRequest("Request was already closed");
            }
            request.Status = "Rejected";
            repository.UpdateFinancialRequest(request);
            return Ok("Task Rejected");
        }
        return BadRequest("Task not found");
    }
}
