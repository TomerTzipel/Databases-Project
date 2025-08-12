using Microsoft.AspNetCore.Mvc;

namespace TriviaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriviaController : ControllerBase
    {
        [HttpGet("question")]
        public async Task<IActionResult> GetQuestion()
        {
            var q = await DatabaseManager.Instance.GetQuestion();
            if (q is null) return NotFound(new { error = "no_questions" });
            return Ok(q);
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetQuestions()
        {
            var list = await DatabaseManager.Instance.GetQuestions();
            if (list is null || list.Count == 0) return NotFound(new { error = "no_questions" });
            return Ok(list);
        }

        [HttpGet("players/exists")]
        public async Task<IActionResult> PlayerExists([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { error = "name_required" });

            var exists = await DatabaseManager.Instance.DoesPlayerExist(name);
            return Ok(new { exists });
        }

        [HttpPost("players")]
        public async Task<IActionResult> AddPlayer([FromBody] Player player)
        {
            if (player is null || string.IsNullOrWhiteSpace(player.Name))
                return BadRequest(new { error = "name_required" });

            await DatabaseManager.Instance.AddPlayer(player);
            return Ok(new { ok = true });
        }

        [HttpPost("players/active")]
        public async Task<IActionResult> SetActive([FromQuery] string name, [FromQuery] bool active)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { error = "name_required" });

            await DatabaseManager.Instance.SetPlayerActive(name, active);
            return Ok(new { ok = true });
        }
    }
}
