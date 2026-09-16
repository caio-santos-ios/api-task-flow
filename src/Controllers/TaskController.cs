using System.Security.Claims;
using to_do_list.src.Interfaces;
using to_do_list.src.Models.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using to_do_list.src.Requests;
using Microsoft.Extensions.Primitives;

namespace to_do_list.src.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController(ITaskService service) : ControllerBase
    {
        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                Dictionary<string, StringValues> query = new(Request.Query);
                query["createdBy"] = UserId;
                Request.Query = new QueryCollection(query);
            }

            ResponseApi<PaginationApi<List<dynamic>>> response = await service.GetAllAsync(new(Request.Query));
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            ResponseApi<dynamic?> response = await service.GetByIdAggregateAsync(id);
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest task)
        {
            if (task == null) return BadRequest("Dados inválidos.");
            task.CreatedBy = UserId;
            ResponseApi<to_do_list.src.Models.Task?> response = await service.CreateAsync(task);
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTaskRequest task)
        {
            if (task == null) return BadRequest("Dados inválidos.");
            task.UpdatedBy = UserId;
            ResponseApi<to_do_list.src.Models.Task?> response = await service.UpdateAsync(task);
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpPut("finish")]
        public async Task<IActionResult> Finish([FromBody] FinishTaskRequest task)
        {
            if (task == null) return BadRequest("Dados inválidos.");
            task.UpdatedBy = UserId;
            ResponseApi<to_do_list.src.Models.Task?> response = await service.FinishAsync(task);
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            ResponseApi<to_do_list.src.Models.Task> response = await service.DeleteAsync(new() { Id = id, DeletedBy = UserId });
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}