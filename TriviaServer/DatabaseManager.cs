using Npgsql;
using System.Security.Cryptography;

namespace TriviaServer
{
    public class DatabaseManager
    {
        private static DatabaseManager? _instance = null;
        private static readonly object _padlock = new object();

        // TODO: Move to appsettings.json and read via IConfiguration
        // Replace the placeholders below with real values:
        private string _connectionString =
            "Host=YOUR_HOST;Port=5432;Username=YOUR_USER;Password=YOUR_PASSWORD;Database=YOUR_DB;SSL Mode=Require;Trust Server Certificate=true";

        private DatabaseManager() { }

        public static DatabaseManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new DatabaseManager();
                }
            }
        }

        public async Task<List<Question>?> GetQuestions()
        {
            try
            {
                var list = new List<Question>();
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"SELECT question, option0, option1, option2, option3, answerindex
                                     FROM public.""Questions""";
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var q = new Question
                    {
                        QuestionText = reader.GetString(reader.GetOrdinal("question")),
                        OptionTexts = new[]
                        {
                            reader.GetString(reader.GetOrdinal("option0")),
                            reader.GetString(reader.GetOrdinal("option1")),
                            reader.GetString(reader.GetOrdinal("option2")),
                            reader.GetString(reader.GetOrdinal("option3")),
                        },
                        AnswerIndex = reader.GetInt32(reader.GetOrdinal("answerindex"))
                    };
                    list.Add(q);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Question?> GetQuestion()
        {
            var list = await GetQuestions();
            if (list == null || list.Count == 0) return null;

            var idx = RandomNumberGenerator.GetInt32(list.Count);
            return list[idx];
        }

        public async Task<bool> DoesPlayerExist(string name)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"SELECT 1 FROM public.""Players""
                                     WHERE name = @name
                                     LIMIT 1;";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name ?? string.Empty);

                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task SetPlayerActive(string name, bool value)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"UPDATE public.""Players""
                                     SET ""isActive"" = @active
                                     WHERE name = @name;";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@active", value);
                cmd.Parameters.AddWithValue("@name", name ?? string.Empty);

                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // optionally log
            }
        }

        public async Task AddPlayer(Player player)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"INSERT INTO public.""Players""
                                     (""name"",""score"",""totalTime"",""isActive"")
                                     VALUES (@name,@score,@totalTime,@isActive);";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", player.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@score", player.Score);
                cmd.Parameters.AddWithValue("@totalTime", player.TotalTime);
                cmd.Parameters.AddWithValue("@isActive", player.IsActive);

                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // optionally log
            }
        }
    }
}
