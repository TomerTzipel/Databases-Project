using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TriviaServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriviaController : ControllerBase
    {
        // GET: api/<TriviaController>
        [HttpGet("GetQuestions")]
        public async Task<IEnumerable<Question>?> GetQuestions()
        {
            List<Question>? QuestionList = await DatabaseManager.Instance.GetQuestions();
            return QuestionList;
        }

        // GET api/<TriviaController>/5
        [HttpGet("{id:int}")]
        public async Task<Question?> Get(int id)
        {
            Question? question = await DatabaseManager.Instance.GetQuestion(id);
            return question;
        }

        // GET api/<TriviaController>/6
        [HttpGet("{name}")]
        public async Task<bool> Get(string name)
        {
            bool result = await DatabaseManager.Instance.DoesPlayerExist(name);
            return result;
        }

        // POST api/<TriviaController>
        [HttpPost("{name}")]
        public async Task Post(string name)
        {
            Player player = new Player();
            player.Name = name;
            player.IsActive = false;
            await DatabaseManager.Instance.AddPlayer(player);
        }

        // PUT api/<TriviaController>/5
        [HttpPut("{name},{value}")]
        public async Task Put(string name,bool value)
        {
            await DatabaseManager.Instance.SetPlayerActive(name,value);
        }

        // DELETE api/<TriviaController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
