using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class UserLoginHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public GameObject RegistrationPanel;
    public GameObject NameTakenText;
    public GameObject WaitingPanel;

    [Header("Server URL")]
    [Tooltip("your url to the server")]
    public string ServerURL = "URL"; // Replace with your server URL

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NameTakenText.SetActive(false);
        WaitingPanel.SetActive(false);

        string savedUsername = PlayerPrefs.GetString("username", "");
        if (!string.IsNullOrEmpty(savedUsername))
        {
            usernameInput.text = savedUsername;
            StartCoroutine(CheckUserTakenRoutine(savedUsername));
        }
        else
        {
            RegistrationPanel.SetActive(true);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSubmitName()
    {
        string username = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }

        RegistrationPanel.SetActive(false);
        StartCoroutine(CheckUserTakenRoutine(username));
    }
    private IEnumerator CheckUserTakenRoutine(string savedUsername)
    {
        NameTakenText.SetActive(false);

        var url = $"{ServerURL}/check_user_taken?username={UnityWebRequest.EscapeURL(savedUsername)}"; // Construct the URL for checking if the user is taken
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"CheckUser Error : {request.error}");
                RegistrationPanel.SetActive(true);
                yield break;
            }

            var response = JsonUtility.FromJson<UserTakenResponse>(request.downloadHandler.text);
            if (response.taken)
            {
                NameTakenText.SetActive(true);
                RegistrationPanel.SetActive(true);
            }
            else
            {
                StartCoroutine(CreateUserRoutine(savedUsername));
            }
        }
    }

    private IEnumerator CreateUserRoutine (string savedUsername)
    {
        var form = new WWWForm();
        form.AddField("username", savedUsername);

        using (var request = UnityWebRequest.Post($"{ServerURL}/createUser", form))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"CreateUser Error : {request.error}");
                RegistrationPanel.SetActive(true);
                yield break;
            }

            var response = JsonUtility.FromJson<CreateUserResponse>(request.downloadHandler.text);
            if (response.success)
            {
                PlayerPrefs.SetString("username", savedUsername);
                PlayerPrefs.Save();
                Debug.Log($"User {savedUsername} created successfully.");
                WaitingPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("Failed to create user.");
                RegistrationPanel.SetActive(true);
            }

        }
    }



    [Serializable]
    private class CreateUserResponse
    {
        public bool success;
        public string message;
    }

    [Serializable]
    private class UserTakenResponse
    {
        public bool taken;
    }


}
