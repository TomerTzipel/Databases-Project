using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class Ron_UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] optionButtons = new Button[4]; 

    // Subscribe from your game logic 
    public event UnityAction<int> AnswerChosen; // passes the chosen option index (0..3)

    // ---------- Methods to update UI text ----------
    public void SetQuestion(string text)
    {
        if (questionText) questionText.text = text ?? string.Empty;
    }

    public void SetOptions(string[] options)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (!optionButtons[i]) continue;

          
            TMP_Text label = optionButtons[i].GetComponentInChildren<TMP_Text>();
            if (label)
            {
                string s = (options != null && i < options.Length) ? options[i] : string.Empty;
                label.text = s ?? string.Empty;
            }
        }
    }

    public void SetQuestionAndOptions(string question, string[] options)
    {
        SetQuestion(question);
        SetOptions(options);
    }

    public void SetOption(int index, string text)
    {
        if (index < 0 || index >= optionButtons.Length) return;
        TMP_Text label = optionButtons[index].GetComponentInChildren<TMP_Text>();
        if (label) label.text = text ?? string.Empty;
    }

    
    public void UI_PickOption0() => AnswerChosen?.Invoke(0);
    public void UI_PickOption1() => AnswerChosen?.Invoke(1);
    public void UI_PickOption2() => AnswerChosen?.Invoke(2);
    public void UI_PickOption3() => AnswerChosen?.Invoke(3);
}
