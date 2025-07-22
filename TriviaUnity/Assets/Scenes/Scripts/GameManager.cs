using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private DataManager dataManager;

    private Coroutine searchCoroutine = null;

    public void StartGame()
    {

    }

    public void SearchGame()
    {
        searchCoroutine = StartCoroutine(dataManager.PollAvailableGame());
    }
    public void CancelSearchGame()
    {
        StopCoroutine(searchCoroutine);
        searchCoroutine = null;
    }
    public void OnQuit()
    {
        Application.Quit();
    }
}
