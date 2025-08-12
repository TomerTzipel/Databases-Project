using Microsoft.AspNetCore.Mvc;
using Npgsql;
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

        [HttpPut("AddPlayer_{name}")]
        public async Task AddPlayer(string name)
        {
            await DatabaseManager.Instance.AddPlayer(name);
        }

        [HttpGet("GetPlayingStatus_{name}")]
        public async Task<bool> GetPlayingStatus(string name)
        {
            bool result = await DatabaseManager.Instance.GetIsPlayerPlaying(name);
            return result;
        }

        [HttpPut("SetPlayingStatus_{name},{value}")]
        public async Task PutPlayingStatus(string name, bool value)
        {
            await DatabaseManager.Instance.SetPlayerPlaying(name, value);
        }

        [HttpGet("GetSearchingPlayer_{searchingName}")]
        public async Task<SearchResult?> GetSearchingPlayer(string searchingName)
        {
            SearchResult? result = await DatabaseManager.Instance.GetSearchingPlayer(searchingName);
            return result;
        }
        [HttpGet("GetOpponent_{name}")]
        public async Task<string?> GetOpponent(string name)
        {
            string? result = await DatabaseManager.Instance.GetOpponent(name);
            return result;
        }

        [HttpPut("SetSearchingStatus_{name},{value}")]
        public async Task SetSearchingStatus(string name, bool value)
        {
            await DatabaseManager.Instance.SetPlayerSearching(name, value);
        }

        [HttpPut("SetPlayerInGame_{name},{oppName}")]
        public async Task PutPlayerInGame_(string name, string oppName)
        {
            await DatabaseManager.Instance.SetPlayerInGame(name, oppName);
        }

        [HttpGet("GetPlayerGameResult_{name}")]
        public async Task<GameResult?> GetPlayerGameResult(string name)
        {
            GameResult? result = await DatabaseManager.Instance.GetPlayerGameResult(name);
            return result;
        }

        [HttpPut("SetPlayerGameResult_{name},{score},{totalTime}")]
        public async Task PutPlayerGameResult(string name, int score, int totalTime)
        {
            await DatabaseManager.Instance.SetPlayerGameResult(name, new GameResult { Score = score,TotalTime = totalTime });
        }
    }
}
