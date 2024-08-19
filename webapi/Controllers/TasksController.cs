using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI;
using WebAPI.DataModels;
using WebAPI.EnumTypes;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly SEPRepository repository;

    public TasksController(SEPRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("PostTask")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "ProductionManager, ServicesManager")]
    public IActionResult PostTask([FromBody] SEPTaskCreateDTO createTask)
    {
        SEPTask task = new SEPTask()
        {
            Description = createTask.Description,
            AssignTo = createTask.AssignTo,
            Reference = createTask.Reference,
            Priority = createTask.Priority,
            Status = "InProgress",
        };
           
        if (repository.AddTask(task))
        {
            return Ok();  // OK = 200;
        }
        return BadRequest("Could not create task");
    }

    [HttpGet("GetTasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "ProductionDepartment, ServicesDepartment, ServicesManager, ProductionManager")]
    public List<SEPTask> GetAssignedTasks()
    {
        List<SEPTask> result = new();
        if (User.IsInRole(UserRoles.ProductionDepartment.ToString()) || User.IsInRole(UserRoles.ServicesDepartment.ToString()))
        {
            result.AddRange(repository.GetTasksForName(User.Identity.Name).Where(x => x.Status == "InProgress").ToList());
        }
        else
        {
            result.AddRange(repository.GetAllTasks());   
        }
        return result;
    }

    [HttpPost("CompletedTask")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "ProductionDepartment, ServicesDepartment, ServicesManager, ProductionManager")]
    public IActionResult CompletedTask([FromBody] IDJSON id)
    {
        SEPTask? task = repository.GetTask(id.Id);
        if (task != null)
        {
            if (task.Status == "InProgress")
            {
                if (User.Identity.Name != task.AssignTo)
                {
                    return BadRequest("Not authorized to complete this task");
                }
                task.Status = "Completed";
            }
            else if (task.Status == "Completed")
            {
                if (!User.IsInRole(UserRoles.ProductionManager.ToString()) && !User.IsInRole(UserRoles.ServicesManager.ToString()))
                {
                    return BadRequest("Not authorized to approve this task");
                }
                task.Status = "Approved";
            }
            else
            {
                return BadRequest("Task already closed");
            }
            repository.UpdateTask(task);
            return Ok("Task Completed");
        }
        return BadRequest("Task not found");
    }

    [HttpPost("RejectTask")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "ServicesManager, ProductionManager")]
    public IActionResult RejectTask([FromBody] IDJSON id)
    {
        SEPTask? task = repository.GetTask(id.Id);
        if (task != null)
        {
            task.Status = "InProgress";
            repository.UpdateTask(task);
            return Ok("Task Rejected");
        }
        return BadRequest("Task not found");
    }
}
