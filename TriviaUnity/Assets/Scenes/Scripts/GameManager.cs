
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

public class GameManager : MonoBehaviour
{
    private List<Question> _questions;

    async void Awake()
    {
        await GetQuestionsList();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    

    // Update is called once per frame
    void Update()
    {
        
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
        json = json.Remove(0,1);
        json = json.Remove(json.Length-1,1);

        string[] questionsJson = json.Split("}",System.StringSplitOptions.RemoveEmptyEntries);

        for(int i = 0; i < questionsJson.Length; i++)
        {
            questionsJson[i] = questionsJson[i] + '}';

            if (questionsJson[i][0] == ',') questionsJson[i] = questionsJson[i].Remove(0, 1);
        }

        _questions = new List<Question>();

        foreach (string question in questionsJson)
        {
            _questions.Add(JsonUtility.FromJson<Question>(question));
        }
        
        foreach (Question question in _questions) 
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
