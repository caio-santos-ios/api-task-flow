using Microsoft.AspNetCore.Mvc;

namespace to_do_list.src.Controllers
{
    [Route("api/check")]
    [ApiController]
    public class CheckController : ControllerBase
    {        
        [HttpGet]
        public IActionResult Check()
        {
            return StatusCode(200, new { message = "API OK" });
        }
    }
}