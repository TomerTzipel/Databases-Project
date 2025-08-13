using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GameOverData
{
    public string PlayerName;
    public int NumCorrect;
    public float DurationSeconds;
    public int Outcome;
}

[System.Serializable]
public class PlayerAnalytics
{
    public long totalGamesPlayed;
    public long totalWins;
    public long totalLosses;
    public long totalTies;
    public double averageDurationSeconds;
}
[System.Serializable]
public class GameResult
{
    public int totalTime;
    public int score;
}
[System.Serializable]
public class QuestionResult
{
    public int time;
    public bool result;
}

[System.Serializable]
public class Question
{
    public string questionText;
    public string[] optionTexts = new string[4];
    public int answerIndex;
}

[System.Serializable]
public class SearchResult
{
    public bool wasFound;
    public string playerName;
}

public class DataManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float serverPollInterval;

    async void Awake()
    {
        await GetQuestionsList();
    }

    public IEnumerator PollGameOver(string opponentName)
    {
        while (true) 
        {
            Task<bool> gameTask = IsPlayerPlaying(opponentName);
            yield return new WaitUntil(() => gameTask.IsCompleted);

            if (!gameTask.Result)
            {
                Task<GameResult> resultTask = GetPlayerResult(opponentName);
                yield return new WaitUntil(() => resultTask.IsCompleted);

                gameManager.CompareGameResults(resultTask.Result);
                break;
            }
        }
    }

    public IEnumerator PollAvailableGame()
    {
        Task<SearchResult> partnerTask = SearchForOpponent(gameManager.Name);
        yield return new WaitUntil(() => partnerTask.IsCompleted);

        Task statusTask;

        if (partnerTask.Result != null)
        {

            if (partnerTask.Result.wasFound)
            {
                statusTask = SetInGame(partnerTask.Result.playerName,gameManager.Name);
                yield return new WaitUntil(() => statusTask.IsCompleted);

                statusTask = SetInGame(gameManager.Name, partnerTask.Result.playerName);
                yield return new WaitUntil(() => statusTask.IsCompleted);
                gameManager.OpponentName = partnerTask.Result.playerName;
                gameManager.StartGame();
                yield break;
            }
        }

        statusTask = SetSearchStatus(true, gameManager.Name);
        yield return new WaitUntil(() => statusTask.IsCompleted);

        uiManager.ShowWaitingPanel(WaitType.SearchingPlayer);

        while (true)
        {
            Task<bool> gameTask = IsPlayerPlaying(gameManager.Name);
            yield return new WaitUntil(() => gameTask.IsCompleted);
            if (gameTask.Result)
                break;
            yield return new WaitForSeconds(serverPollInterval);
        }

        Task<string> oppTask = GetOpponent(gameManager.Name);
        yield return new WaitUntil(() => oppTask.IsCompleted);

        gameManager.OpponentName = oppTask.Result;
        gameManager.StartGame();  
    }
    public async Task<bool> DoesPlayerExist(string name)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://localhost:7170/api/Trivia/DoesPlayerExist_{name}");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return false;
        }

        string result = www.downloadHandler.text;
        return result == "true";
    }
    public async Task AddPlayer(string name)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://localhost:7170/api/Trivia/AddPlayer_{name}","");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }

    //Returns true and the name of the players that was found with isSearching = true
    private async Task<SearchResult> SearchForOpponent(string name)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://localhost:7170/api/Trivia/GetSearchingPlayer_{name}");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return null;
        }

        string json = www.downloadHandler.text;
        SearchResult result = JsonUtility.FromJson<SearchResult>(json);
        return result;
    }

    //DB Effect - Returns isPlaying of the given name
    private async Task<bool> IsPlayerPlaying(string name)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://localhost:7170/api/Trivia/GetPlayingStatus_{name}");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return false;
        }

        string result = www.downloadHandler.text;
        return result == "true";
    }

    //DB Effect - Returns the game result of the given player name
    public async Task<GameResult> GetPlayerResult(string name)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://localhost:7170/api/Trivia/GetPlayerGameResult_{name}");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return null;
        }

        string json = www.downloadHandler.text;

        GameResult result = JsonUtility.FromJson<GameResult>(json);
        return result;
    }

    //DB Effect - Returns the opponent's name of the given player name
    public async Task<string> GetOpponent(string name)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://localhost:7170/api/Trivia/GetOpponent_{name}");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return null;
        }

        string result = www.downloadHandler.text;
        return result;
    }

    //DB Effect - Sets the game result of the given player name
    public async Task SetPlayerResult(string name,GameResult result)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://localhost:7170/api/Trivia/SetPlayerGameResult_{name},{result.score},{result.totalTime}","");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }
    //DB Effect - Sets isSearching = value
    public async Task SetSearchStatus(bool value,string name)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://localhost:7170/api/Trivia/SetSearchingStatus_{name},{value}","");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }

    //DB Effect - Sets isPlaying = value
    public async Task SetPlayingStatus(bool value, string name)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://localhost:7170/api/Trivia/SetPlayingStatus_{name},{value}","");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }

    //DB Effect - sets isSearching = false, isPlaying = true , oppName = oppName
    //Only to the user whose name = myName
    private async Task SetInGame(string myName,string oppName)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://localhost:7170/api/Trivia/SetPlayerInGame_{myName},{oppName}", "");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }

    private async Task GetQuestionsList()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetQuestions");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }

        string json = www.downloadHandler.text;
        json = json[1..];
        json = json[..^1];

        string[] questionsJson = json.Split("}",System.StringSplitOptions.RemoveEmptyEntries);

        for(int i = 0; i < questionsJson.Length; i++)
        {
            questionsJson[i] = questionsJson[i] + '}';

            if (questionsJson[i][0] == ',') questionsJson[i] = questionsJson[i].Remove(0, 1);
        }

        gameManager.Questions = new List<Question>();

        foreach (string question in questionsJson)
        {
            gameManager.Questions.Add(JsonUtility.FromJson<Question>(question));
        }   
    }

    //Fire Base Methods:
    public async Task<PlayerAnalytics> GetPlayerAnalytics(string playerName)
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://localhost:5180/api/Analytics/GetPlayerAnalytics_{playerName}");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return null;
        }

        var json = www.downloadHandler.text;
        PlayerAnalytics data = JsonUtility.FromJson<PlayerAnalytics>(json);
        return data;
    }

    public async Task SavePlayerAnalytics(GameOverData data)
    {
        string json = JsonUtility.ToJson(data);
        using var req = new UnityWebRequest($"http://localhost:5180/api/Analytics/GameOver", UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(req.error);
            return;
        }
    }
    

}
