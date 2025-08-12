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
            if (dto is null) return BadRequest(new { error = "body_required" });
            if (string.IsNullOrWhiteSpace(dto.PlayerName))
                return BadRequest(new { error = "playerName_required" });
            if (string.IsNullOrWhiteSpace(dto.Result))
                return BadRequest(new { error = "result_required" });

            var r = dto.Result.Trim().ToLowerInvariant();
            if (r != "win" && r != "loss" && r != "tie")
                return BadRequest(new { error = "result_invalid", allowed = new[] { "win", "loss", "tie" } });

            try
            {
                var normalized = await FirestoreAnalytics.Instance.UpdatePlayerStats(dto);
                // Echo back the normalized result string
                return Ok(new { ok = true, result = normalized });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "analytics_failed", detail = ex.Message });
            }
        }
    }
}
