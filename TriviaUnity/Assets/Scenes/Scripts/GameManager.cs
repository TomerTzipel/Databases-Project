using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

public enum GameResultType
{
    Win,Lose,Tie
}

public enum WaitType
{
    OpponentFinish,SearchingPlayer
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private DataManager dataManager;

    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private int timerDuration;
    public List<Question> Questions { get; set; }
    public QuestionResult[] Results { get; set; }

    public string Name { get; set; } 

    private GameResult _currentGameResult;

    private int _currentQuestion;
    private string _opponentName;

    private Coroutine searchCoroutine = null;

    private void Awake()
    {
        uiManager.ShowMainMenuPanel();

        if(!PlayerPrefs.HasKey("Name")) uiManager.ActivateInsertNamePanel(true);
    }
    public async void IsNameAlreadyUsed()
    {
        string playerName = uiManager.nameInputField.text;
        Task<bool> checkTask = dataManager.DoesPlayerExist(playerName);
        await checkTask;

        if (checkTask.Result)
        {
            uiManager.ActivateNameAlreadyUsedWarning(true);
            return;
        }

        Name = PlayerPrefs.GetString("Name");
        SavePlayerName(playerName);
    }

    public async void SavePlayerName(string name)
    {
        uiManager.ActivateNameAlreadyUsedWarning(false);
        Name = name;
        PlayerPrefs.SetString("Name", name);
        await dataManager.AddPlayer(name);
        uiManager.ActivateInsertNamePanel(false);
    }

    public int HandleAnswer(int asnwerIndex)
    {
        countdownTimer.StopCountdown();
        int correctAnswer = Questions[_currentQuestion].answerIndex;
        bool result = asnwerIndex == correctAnswer;

        Results[_currentQuestion].result = result;
        Results[_currentQuestion].time = timerDuration - countdownTimer.CurrentTime;

        return correctAnswer;
    }

    public void NextQuestion()
    {
        _currentQuestion++;

        if(_currentQuestion >= Questions.Count)
        {
            HandleTriviaCompletion();
            return;
        }

        uiManager.LoadQuestionAndOptions(Questions[_currentQuestion]);
        countdownTimer.Run(timerDuration);
    }

    public void StartGame()
    {
        uiManager.ShowTriviaPanel();
        Results = new QuestionResult[Questions.Count];
        _currentQuestion = -1;
        NextQuestion();
    }

    public void CompareGameResults(GameResult oppResult)
    {
        if(oppResult.score > _currentGameResult.score)
        {
            uiManager.ShowResultPanel(GameResultType.Lose);
            return;
        }

        if (oppResult.score < _currentGameResult.score)
        {
            uiManager.ShowResultPanel(GameResultType.Win);
            return;
        }

        if (oppResult.totalTime < _currentGameResult.totalTime)
        {
            uiManager.ShowResultPanel(GameResultType.Lose);
            return;
        }

        if (oppResult.totalTime > _currentGameResult.totalTime)
        {
            uiManager.ShowResultPanel(GameResultType.Win);
            return;
        }

        uiManager.ShowResultPanel(GameResultType.Tie);
    }

    //Should be on a button for the results panel 
    public void FinishGame()
    {
        uiManager.ShowMainMenuPanel();
        //Send analytics to oron shyte
    }

    public void SearchGame()
    {
        searchCoroutine = StartCoroutine(dataManager.PollAvailableGame());
    }
    public async void CancelSearchGame()
    {
        StopCoroutine(searchCoroutine);
        searchCoroutine = null;
        uiManager.ShowMainMenuPanel();
        Task statusTask = dataManager.SetSearchStatus(false, Name);
        await statusTask;   
    }
    public void OnQuit()
    {
        Application.Quit();
    }

    private async void HandleTriviaCompletion()
    {
        _currentGameResult.totalTime = 0;
        _currentGameResult.score = 0;

        foreach (QuestionResult result in Results)
        {
            _currentGameResult.totalTime += result.time;
            if (result.result) _currentGameResult.score++;
        }
        
        await dataManager.SetPlayerResult(Name, _currentGameResult);
        await dataManager.SetPlayingStatus(false, Name);
        StartCoroutine(dataManager.PollGameOver(_opponentName));
    }
}
