using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

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
    [field:SerializeField] public bool WasFound { get; set; }
    [field: SerializeField] public string PlayerName { get; set; }
}

public class DataManager : MonoBehaviour
{
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
        Task<SearchResult> partnerTask = SearchForPartner();
        yield return new WaitUntil(() => partnerTask.IsCompleted);

        Task statusTask;

        if (partnerTask.Result != null)
        {
            if (partnerTask.Result.WasFound)
            {
                statusTask = SetInGame(partnerTask.Result.PlayerName,gameManager.Name);
                yield return new WaitUntil(() => statusTask.IsCompleted);

                statusTask = SetInGame(gameManager.Name, partnerTask.Result.PlayerName);
                yield return new WaitUntil(() => statusTask.IsCompleted);

                gameManager.StartGame();
                yield break;
            }
        }

        statusTask = SetSearchStatus(true, gameManager.Name);
        yield return new WaitUntil(() => statusTask.IsCompleted);

        while (true)
        {
            Task<bool> gameTask = IsPlayerPlaying(gameManager.Name);
            yield return new WaitUntil(() => gameTask.IsCompleted);
            if (gameTask.Result)
                break;
            yield return new WaitForSeconds(serverPollInterval);
        }

        gameManager.StartGame();  
    }
    public async Task<bool> DoesPlayerExist(string name)
    {
        //Wrong URL
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return false;
        }

        string json = www.downloadHandler.text;

        bool result = JsonUtility.FromJson<bool>(json);
        return result;
    }
    public async Task AddPlayer(string name)
    {
        //Wrong url
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }

    //Returns true and the name of the players that was found with isSearching = true
    private async Task<SearchResult> SearchForPartner()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
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
        //Wrong URL
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return false;
        }

        string json = www.downloadHandler.text;

        bool result = JsonUtility.FromJson<bool>(json);
        return result;
    }

    //DB Effect - Returns the game result of the given player name
    public async Task<GameResult> GetPlayerResult(string name)
    {
        //Wrong url
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
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
    //DB Effect - Returns the game result of the given player name
    public async Task SetPlayerResult(string name,GameResult result)
    {
        //Wrong url
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }
    //DB Effect - sets isSearching = value
    public async Task SetSearchStatus(bool value,string name)
    {
        //Wrong url
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }
    public async Task SetPlayingStatus(bool value, string name)
    {
        //Wrong url
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            return;
        }
    }

    //DB Effect - sets isSearching = false, isPlaying = true , oppName = oppName
    //Only to the user whose name = myName
    private async Task SetInGame(string myName,string oppName)//OK
    {
        //Wrong url
        UnityWebRequest www = UnityWebRequest.Get("https://localhost:7170/api/Trivia/GetSearchingPlayer");
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
        
        foreach (Question question in gameManager.Questions) 
        {
            Debug.Log($"Question {question.questionText}");
            Debug.Log($"Answer1 {question.optionTexts[0]}");
            Debug.Log($"Answer2 {question.optionTexts[1]}");
            Debug.Log($"Answer3 {question.optionTexts[2]}");
            Debug.Log($"Answer4 {question.optionTexts[3]}");
            Debug.Log($"Answer {question.answerIndex}");
            Debug.Log("");
        }
    }

}
