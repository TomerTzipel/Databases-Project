using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


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
    public bool WasFound { get; set; }
    public string PlayerName { get; set; }
}

public class DataManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Ron_UIManager uiManager;

    [SerializeField] private float serverPollInterval;

    public List<Question> Questions {get; private set;}

    async void Awake()
    {
        await GetQuestionsList();
    }

    public IEnumerator PollAvailableGame()
    {
        Task<SearchResult> partnerTask = SearchForPartner();
        yield return new WaitUntil(() => partnerTask.IsCompleted);

        if(partnerTask.Result != null)
        {
            bool wasFound = partnerTask.Result.WasFound;

            if (wasFound)
            {
                //Set up a game in the DB
                //And join it
                gameManager.StartGame();
                yield break;
            }
        }

        Task searchStatusTask = SetSearchStatus(true);
        yield return new WaitUntil(() => searchStatusTask.IsCompleted);

        while (!IsGameAvailable())
        {
            yield return new WaitForSeconds(serverPollInterval);
        }

        searchStatusTask = SetSearchStatus(false);
        yield return new WaitUntil(() => searchStatusTask.IsCompleted);

        //Join the game in the DB

        gameManager.StartGame();
    }

    private bool IsGameAvailable()
    {
        return true;
        //Check in the database if another player is looking for a game.
    }

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
    private async Task SetSearchStatus(bool value)
    {
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

        Questions = new List<Question>();

        foreach (string question in questionsJson)
        {
            Questions.Add(JsonUtility.FromJson<Question>(question));
        }
        
        foreach (Question question in Questions) 
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
