using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float startTime = 30f;

    public event UnityAction TimerEnded; // Event fired when timer hits 0

    private float currentTime;
    private bool isRunning;

    private void Start()
    {
        StartCountdown(startTime);
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            UpdateTimerDisplay();

            TimerEnded?.Invoke(); 
            return;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timerText.text = seconds.ToString();
    }

    public void StartCountdown(float time)
    {
        startTime = time;
        currentTime = time;
        isRunning = true;
        UpdateTimerDisplay();
    }

    public void StopCountdown()
    {
        isRunning = false;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}
