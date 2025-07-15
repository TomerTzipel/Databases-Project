using Npgsql;

namespace TriviaServer
{
    public class DatabaseManager
    {
        private static DatabaseManager? _instance = null;
        private static readonly object _padlock = new object();
        private string _connectionString = "Host=aws-0-eu-central-1.pooler.supabase.com;Database=postgres;Username=postgres.xivoeutxghwfwoncdqzo;Password=#kLsZWP?S5riK6T;Port=5432;SSL Mode=Require;Trust Server Certificate=true";
       
        private DatabaseManager() { }

        public static DatabaseManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseManager();
                    }
                    return _instance;
                }

            }
        }

        public async Task<Question?> GetQuestion(int id)
        {
            try
            {
                Question question = new Question();
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT * FROM \"public\".\"Questions\" WHERE id = " + id;
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    question.QuestionText = reader.GetString(reader.GetOrdinal("question"));
                    question.OptionTexts[0] = reader.GetString(reader.GetOrdinal("answer1"));
                    question.OptionTexts[1] = reader.GetString(reader.GetOrdinal("answer2"));
                    question.OptionTexts[2] = reader.GetString(reader.GetOrdinal("answer3"));
                    question.OptionTexts[3] = reader.GetString(reader.GetOrdinal("answer4"));
                    question.AnswerIndex = reader.GetInt16(reader.GetOrdinal("correctAnswer")) - 1;
                }
                return question;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Question>?> GetQuestions()
        {
            try
            {
                List<Question> questionList = new List<Question>();
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT * FROM \"public\".\"Questions\"";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    Question question = new Question();
                    question.QuestionText = reader.GetString(reader.GetOrdinal("question"));
                    question.OptionTexts[0] = reader.GetString(reader.GetOrdinal("answer1"));
                    question.OptionTexts[1] = reader.GetString(reader.GetOrdinal("answer2"));
                    question.OptionTexts[2] = reader.GetString(reader.GetOrdinal("answer3"));
                    question.OptionTexts[3] = reader.GetString(reader.GetOrdinal("answer4"));
                    question.AnswerIndex = reader.GetInt16(reader.GetOrdinal("correctAnswer")) - 1;
                    questionList.Add(question);
                }
                return questionList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DoesPlayerExist(string name)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"SELECT COUNT(*) FROM \"public\".\"Players\" WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                Object? result = await command.ExecuteScalarAsync();

                Int32 count = 0;
                if (result != null)
                {
                    count = Convert.ToInt32(result);

                }

                return count > 0;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task SetPlayerActive(string name,bool value)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"UPDATE \"public\".\"Players\" SET \"isActive\"= {value} WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
            }
        }

        public async Task AddPlayer(Player player)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "INSERT INTO \"public\".\"Players\" (\"name\",\"score\",\"totalTime\",\"isActive\") " +
                    $"VALUES ('{player.Name}',{player.Score},{player.TotalTime},{player.IsActive})";

                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                
            }   
        }



        /*public async Task UpdatePlayer(Player player,int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Query Players table
            string query = $"UPDATE \"public\".\"Players\" " +
                           $"SET name = {player.Name}, " +
                           $"WHERE id = {id}";
        }*/
    }
}
