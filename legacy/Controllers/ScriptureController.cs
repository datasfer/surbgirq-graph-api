using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.DataAccessApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScriptureController : ControllerBase
    {
        IDriver _driver;
        private readonly ILogger<ScriptureController> _logger;
        public ScriptureController( IDriver driver,
                                    ILogger<ScriptureController> logger) 
        {
            _driver = driver;
            _logger = logger;
        }

        [HttpGet]
        public void get()
        {
            _logger.LogInformation("Test");
        }
    }
}
