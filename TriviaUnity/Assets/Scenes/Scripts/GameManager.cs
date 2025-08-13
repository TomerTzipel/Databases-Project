using System.Collections.Generic;
using System.Threading.Tasks;
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

    private GameResult _currentGameResult = new GameResult();

    private int _currentQuestion;
    public string OpponentName { get; set; }

    private Coroutine searchCoroutine = null;

    private async void Awake()
    {
        uiManager.ShowMainMenuPanel();

        if (!PlayerPrefs.HasKey("Name"))
        {
            uiManager.ActivateInsertNamePanel(true);
            return;
        }

        Name = PlayerPrefs.GetString("Name");
        Task<PlayerAnalytics> analyticsTask = dataManager.GetPlayerAnalytics(Name);
        await analyticsTask;

        uiManager.UpdateStats(analyticsTask.Result);

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

        SavePlayerName(playerName);
    }

    private async void SavePlayerName(string name)
    {
        uiManager.ActivateNameAlreadyUsedWarning(false);

        Task playerTask = dataManager.SavePlayerAnalytics(new GameOverData { PlayerName = name,NumCorrect = 0,DurationSeconds = 0,Outcome = -1});
        await playerTask;

        playerTask = dataManager.AddPlayer(name);
        await playerTask;

        Name = name;
        PlayerPrefs.SetString("Name", name);
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

        for (int i = 0; i < Results.Length; i++)
        {
            Results[i] = new QuestionResult();
        }
        _currentQuestion = -1;
        NextQuestion();
    }

    public async void CompareGameResults(GameResult oppResult)
    {
        GameResultType resultType = GameResultType.Tie;

        if (oppResult.score > _currentGameResult.score)
        {
            resultType = GameResultType.Lose;
            uiManager.ShowResultPanel(GameResultType.Lose);
        }
        else if (oppResult.score < _currentGameResult.score)
        {
            resultType = GameResultType.Win;
            uiManager.ShowResultPanel(GameResultType.Win);
        }
        else if (oppResult.totalTime < _currentGameResult.totalTime)
        {
            resultType = GameResultType.Lose;
            uiManager.ShowResultPanel(GameResultType.Lose);
        }
        else if (oppResult.totalTime > _currentGameResult.totalTime)
        {
            resultType = GameResultType.Win;
            uiManager.ShowResultPanel(GameResultType.Win);
        }

        Task analyticsTask = dataManager.SavePlayerAnalytics(new GameOverData
        {
            PlayerName = Name,
            Outcome = (int)resultType,
            NumCorrect = _currentGameResult.score,
            DurationSeconds = _currentGameResult.totalTime
        });

        await analyticsTask;

        Task<PlayerAnalytics> statsTask = dataManager.GetPlayerAnalytics(Name);
        await statsTask;

        uiManager.UpdateStats(statsTask.Result);
        uiManager.ShowResultPanel(resultType);
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
        uiManager.ShowWaitingPanel(WaitType.OpponentFinish);
        StartCoroutine(dataManager.PollGameOver(OpponentName));
    }
}
