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
            if (string.IsNullOrWhiteSpace(dto.PlayerName))
                return BadRequest(new { error = "playerName required" });

            await FirestoreAnalytics.Instance.LogGameOver(dto);
            return Ok(new { ok = true });
        }

        [HttpGet("GetPlayerAnalytics_{name}")]
        public async Task<PlayerAnalytics?> GetPlayerAnalytics(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await FirestoreAnalytics.Instance.GetPlayerAnalytics(name);
        }
    }
}
