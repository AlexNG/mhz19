using System.Net;
using System.Threading;
using Mhz.Core;
using Microsoft.AspNetCore.Mvc;

namespace Mhz.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ICore core;
        private volatile bool busy;

        public ValuesController(ICore core)
        {
            this.core = core;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<int> Get()
        {
            if (busy)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            busy = true;
            Thread.Sleep(750);
            busy = false;
            return // core.Ppm;
                core.SendCommandAndGetPpm();
        }
    }
}
