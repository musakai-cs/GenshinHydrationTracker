using GenshinHydrationTracker.Attributes;
using GenshinHydrationTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenshinHydrationTracker.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [ApiKeyAuth]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class GenshinTrackerController(DiscordHydrationWorker worker) : ControllerBase
    {
        private readonly DiscordHydrationWorker _worker = worker ?? throw new NullReferenceException(nameof(worker));

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult StartTracker()
        {
            _worker.StartHydrationReminders();
            return Ok(new { message = "Genshin zapnut, časovač běží." });
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult StopTracker()
        {
            _worker.StopHydrationReminders();
            return Ok(new { message = "Genshin vypnut, časovač zastaven." });
        }

        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetStatus()
        {
            var isRunning = _worker.IsHydrationRemindersRunning();
            return Ok(new { status = isRunning ? "Časovač běží." : "Časovač zastaven." });
        }
    }
}
