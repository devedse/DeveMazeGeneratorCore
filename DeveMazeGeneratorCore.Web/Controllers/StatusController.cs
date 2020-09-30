using DeveMazeGeneratorCore.Web.Status;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DeveMazeGeneratorCore.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;

        public StatusController(ILogger<StatusController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Task<StatusModel> GetAsync()
        {
            _logger.Log(LogLevel.Information, "### Status Controller Get() called");

            var statusModel = StatusObtainer.GetStatus();
            return Task.FromResult(statusModel);
        }
    }
}
