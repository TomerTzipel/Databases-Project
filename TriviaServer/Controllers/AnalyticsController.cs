using Microsoft.AspNetCore.Mvc;
using TriviaServer.Models;
using TriviaServer.Services;

namespace TriviaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        [HttpPost("GameOver")]
        public async Task<IActionResult> GameOver([FromBody] GameOverDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PlayerId))
                return BadRequest(new { error = "playerId required" });

            await FirestoreAnalytics.Instance.LogGameOver(dto);
            return Ok(new { ok = true, matchId = string.IsNullOrWhiteSpace(dto.MatchId) ? "generated" : dto.MatchId });
        }
    }
}
