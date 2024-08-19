using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI;
using WebAPI.DataModels;
using WebAPI.EnumTypes;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class RecruitmentRequestsController : ControllerBase
{
    private readonly SEPRepository repository;

    public RecruitmentRequestsController(SEPRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("PostRecruitmentRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "ProductionManager, ServicesManager, FinancialManager, AdministrationManager")]
    public IActionResult PostTask([FromBody] CreateRecruitmentRequestDTO createTask)
    {
        RecruitmentRequest request = new()
        {
            JobDescription = createTask.JobDescription,
            JobTitle = createTask.JobTitle,
            Yoe = createTask.Yoe,
            Department = createTask.Department,
            ContractType = createTask.ContractType,
            Status = "InProgress",
            CreatedBy = User.Identity.Name,
        };
           
        if (repository.AddRecruitmentRequest(request))
        {
            return Ok();  // OK = 200;
        }
        return BadRequest("Could not create task");
    }

    [HttpGet("GetRecruitmentRequests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "AdministrationManager, FinancialManager, ServicesManager, ProductionManager, HumanResourcesManager")]
    public List<RecruitmentRequest> GetRecruitmentRequests()
    {
        List<RecruitmentRequest> result = new();
        result.AddRange(repository.GetRecruitments());
        if (User.IsInRole(UserRoles.HumanResourcesManager.ToString()))
        {
            result = result.Where(x => x.Status == "InProgress").ToList();
        }
        else
        {
            result = result.Where(x => x.CreatedBy == User.Identity.Name).ToList();
        }
        return result;
    }

    [HttpPost("ApproveRecruitmentRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "HumanResourcesManager")]
    public IActionResult ApproveRecruitmentRequest([FromBody] IDJSON id)
    {
        RecruitmentRequest? request = repository.GetRecruitment(id.Id);
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
            repository.UpdateRecruitmentRequest(request);
            return Ok("Task Completed");
        }
        return BadRequest("Task not found");
    }

    [HttpPost("RejectRecruitmentRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "HumanResourcesManager")]
    public IActionResult RejectRecruitmentRequest([FromBody] IDJSON id)
    {
        RecruitmentRequest? request = repository.GetRecruitment(id.Id);
        if (request != null)
        {
            if (request.Status != "InProgress")
            {
                return BadRequest("Request was already closed");
            }
            request.Status = "Rejected";
            repository.UpdateRecruitmentRequest(request);
            return Ok("Task Rejected");
        }
        return BadRequest("Task not found");
    }
}
