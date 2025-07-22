using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TriviaServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriviaController : ControllerBase
    {
        [HttpGet("GetQuestions")]
        public async Task<IEnumerable<Question>?> GetQuestions()
        {
            List<Question>? QuestionList = await DatabaseManager.Instance.GetQuestions();
            return QuestionList;
        }

        [HttpGet("GetQuestion_{id:int}")]
        public async Task<Question?> GetQuestion(int id)
        {
            Question? question = await DatabaseManager.Instance.GetQuestion(id);
            return question;
        }

        [HttpGet("DoesPlayerExist_{name}")]
        public async Task<bool> DoesPlayerExist(string name)
        {
            bool result = await DatabaseManager.Instance.DoesPlayerExist(name);
            return result;
        }

        [HttpPost("AddPlayer_{name}")]
        public async Task AddPlayer(string name)
        {
            Player player = new Player();
            player.Name = name;
            player.IsActive = false;
            await DatabaseManager.Instance.AddPlayer(player);
        }

        [HttpPut("SetSearchingStatus_{name},{value}")]
        public async Task SetSearchingStatus(string name,bool value)
        {
            await DatabaseManager.Instance.SetPlayerSearching(name,value);
        }

        [HttpGet("GetSearchingPlayer")]
        public async Task<SearchResult?> GetSearchingPlayer()
        {
            SearchResult? result = await DatabaseManager.Instance.GetSearchingPlayer();
            return result;
        }

        /*[HttpPut("SetPlayingStatus_{name},{value}")]
        public async Task PutPlayingStatus(string name, bool value)
        {
            await DatabaseManager.Instance.SetPlayerPlaying(name, value);
        }*/

        [HttpDelete("RemovePlayer_{id}")]
        public void Delete(int id)
        {

        }
    }
}
