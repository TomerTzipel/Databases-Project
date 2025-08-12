using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameManager gameManager;

    [Header("Main Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject insertNamePanel;
    [field:SerializeField] public TMP_InputField nameInputField { get; private set; }
    [SerializeField] private GameObject nameAlreadyUsedMessege;

    [Header("Trivia Panel")]
    [SerializeField] private GameObject triviaPanel;
    [SerializeField] private TMP_Text questionTextTitle;
    [SerializeField] private Image[] optionButtonsImage = new Image[4];
    [SerializeField] private TMP_Text[] optionButtonsText = new TMP_Text[4];
    [SerializeField] private float answerRevealDuration;

    private Color _buttonsOriginalColor;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTextTitle;
    [Header("Waiting Panel")]
    [SerializeField] private GameObject waitPanel;
    [SerializeField] private TMP_Text waitTextTitle;
    [SerializeField] private GameObject cancelSearchButton;

    private void Awake()
    {
        _buttonsOriginalColor = optionButtonsImage[0].color;
    }
    public void LoadQuestionAndOptions(Question question)
    {
        questionTextTitle.text = question.questionText;
        for (int i = 0; i < question.optionTexts.Length; i++)
            optionButtonsText[i].text = question.optionTexts[i];
    }

    public void OnOptionClick(int optionNumber)
    {
        int correctAnswer = gameManager.HandleAnswer(optionNumber);

        optionButtonsImage[correctAnswer].color = Color.green;

        if (correctAnswer != optionNumber)
        {
            optionButtonsImage[optionNumber].color = Color.red;
        }

        StartCoroutine(AnswerRevealDuration(answerRevealDuration));
    }

    public IEnumerator AnswerRevealDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        foreach (Image image in optionButtonsImage)
        {
            image.color = _buttonsOriginalColor;
        }
        gameManager.NextQuestion();
    }
    public void ShowWaitingPanel(WaitType waitType)
    {
        HideAllPanles();
        waitPanel.SetActive(true);

        string waitText = "";
        switch (waitType)
        {
            case WaitType.OpponentFinish:
                waitText = "Waiting for opponent to finish...";
                cancelSearchButton.SetActive(false);
                break;
            case WaitType.SearchingPlayer:
                waitText = "Searching for an opponent...";
                cancelSearchButton.SetActive(true);
                break;
        }

        waitTextTitle.text = waitText;
    }
    public void ShowMainMenuPanel()
    {
        HideAllPanles();
        mainMenuPanel.SetActive(true);
    }

    public void ShowTriviaPanel()
    {
        HideAllPanles();
        triviaPanel.SetActive(true);
    }

    public void ShowResultPanel(GameResultType resultType)
    {
        HideAllPanles();
        resultPanel.SetActive(true);
        string resultText = "";
        switch (resultType)
        {
            case GameResultType.Win:
                resultText = "YOU WON!";
                break;
            case GameResultType.Lose:
                resultText = "YOU LOST...";
                break;
            case GameResultType.Tie:
                resultText = "IT WAS A TIE?!";
                break;
        }

        resultTextTitle.text = resultText; 
    }

    public void ActivateInsertNamePanel(bool value)
    {
        insertNamePanel.SetActive(value);
    }
    public void ActivateNameAlreadyUsedWarning(bool value)
    {
        nameAlreadyUsedMessege.SetActive(value);
    }
    private void HideAllPanles()
    {
        resultPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        waitPanel.SetActive(false);
        triviaPanel.SetActive(false);
    }
}
