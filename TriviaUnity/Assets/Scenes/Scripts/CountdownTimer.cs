using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    public event UnityAction TimerEnded;

    public int CurrentTime { get; private set; }

    public void Run(int duration)
    {
        CurrentTime = duration;
        StartCoroutine(StartCountdown(duration));
    }

    private IEnumerator StartCountdown(int duration)
    {
        while(CurrentTime >= 0)
        {
            UpdateTimerDisplay();
            yield return new WaitForSeconds(1);
            CurrentTime--;
        }

        TimerEnded.Invoke();
    }
    private void UpdateTimerDisplay()
    {
        timerText.text = CurrentTime.ToString();
    }

    public void StopCountdown()
    {
        StopAllCoroutines();
        UpdateTimerDisplay();
    }
}
