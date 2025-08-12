using Npgsql;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;

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

                string query = "SELECT * FROM \"public\".\"Questions\" " +
                               "WHERE id = " + id;
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

                string query = $"SELECT COUNT(*) FROM \"public\".\"Players\" " +
                               $"WHERE name = \'{name}\'";
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
        public async Task<string?> GetOpponent(string name)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"SELECT \"oppName\" FROM \"public\".\"Players\" " +
                               $"WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();


                if (!reader.HasRows)
                {
                    return null;
                }

                reader.Read();
                return reader.GetString(0);

            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<SearchResult?> GetSearchingPlayer(string searchingName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"SELECT name FROM \"public\".\"Players\" " +
                               $"WHERE \"isSearching\" = true AND NOT name = \'{searchingName}\'";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                SearchResult result = new SearchResult();

                if (!reader.HasRows)
                {
                    result.WasFound = false;
                    result.PlayerName = "";
                }
                else
                {
                    reader.Read();
                    result.WasFound = true;
                    result.PlayerName = reader.GetString(0);
                }

                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SetPlayerSearching(string name,bool value)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"UPDATE \"public\".\"Players\" " +
                               $"SET \"isSearching\"= {value} " +
                               $"WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
            }
        }
        public async Task<bool> GetIsPlayerPlaying(string name)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"SELECT \"isPlaying\" FROM \"public\".\"Players\" " +
                               $"WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows) return false;

                reader.Read();
                return reader.GetBoolean(0);
            }
            catch (Exception)
            { 
                return false; 
            }
        }

        public async Task SetPlayerPlaying(string name, bool value)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"UPDATE \"public\".\"Players\" " +
                               $"SET \"isPlaying\"= {value} " +
                               $"WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            { }
        }

        public async Task SetPlayerInGame(string originName,string oppName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"UPDATE \"public\".\"Players\" " +
                               $"SET \"oppName\"= \'{oppName}\', " +
                               $"\"isSearching\"= false, " +
                               $"\"isPlaying\"= true " +
                               $"WHERE name = \'{originName}\'";
                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            { }
        }
        public async Task<GameResult?> GetPlayerGameResult(string name)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"SELECT \"score\",\"totalTime\" FROM \"public\".\"Players\" " +
                               $"WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows) return null;

                reader.Read();
                GameResult result = new GameResult();
                result.Score = reader.GetInt32(reader.GetOrdinal("score"));
                result.TotalTime = reader.GetInt32(reader.GetOrdinal("totalTime"));
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SetPlayerGameResult(string name, GameResult result)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = $"UPDATE \"public\".\"Players\" " +
                               $"SET \"score\"= {result.Score}, " +
                               $"\"totalTime\"= {result.TotalTime} " +
                               $"WHERE name = \'{name}\'";
                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            { }
        }

        public async Task AddPlayer(string name)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "INSERT INTO \"public\".\"Players\" (\"name\") " + $"VALUES ('{name}')";

                using var command = new NpgsqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            { }   
        }
    }
}
