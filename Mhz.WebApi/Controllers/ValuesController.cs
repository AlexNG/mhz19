using Microsoft.AspNetCore.Mvc;

namespace Mhz.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<int> Get()
        {
            return Core.Core.Instance.Ppm;
        }
    }
}
