using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TMP_Text errorText;

    [Header("API URLs")]
    public string loginUrl = "http://localhost:5136/api/auth/login";
    public string registerUrl = "http://localhost:5136/api/auth/register";

    void Start()
    {
        errorText.text = "";
        loginButton.onClick.AddListener(() => StartCoroutine(SendLoginRequest()));
        registerButton.onClick.AddListener(() => StartCoroutine(SendRegisterRequest()));
    }

    IEnumerator SendLoginRequest()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Gebruikersnaam en wachtwoord zijn verplicht.";
            yield break;
        }

        errorText.text = "Bezig met inloggen...";

        LoginRequest payload = new LoginRequest { Username = username, Password = password };
        string jsonData = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                PlayerPrefs.SetString("auth_token", response.token);
                SceneManager.LoadScene("WorldEditScene");
            }
            else
            {
                errorText.text = request.downloadHandler.text;
                Debug.LogError("Login error: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator SendRegisterRequest()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Gebruikersnaam en wachtwoord zijn verplicht.";
            yield break;
        }

        if (!IsValidPassword(password))
        {
            errorText.text = "Wachtwoord moet minimaal 10 tekens bevatten, met hoofdletter, kleine letter, cijfer en speciaal teken.";
            yield break;
        }

        LoginRequest payload = new LoginRequest { Username = username, Password = password };
        string jsonData = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(registerUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
            {
                errorText.text = "Registratie gelukt! Je kunt nu inloggen.";
            }
            else
            {
                errorText.text = request.downloadHandler.text;
                Debug.LogError("Register error: " + request.downloadHandler.text);
            }
        }
    }

    bool IsValidPassword(string password)
    {
        if (password.Length < 10) return false;

        bool hasLower = false, hasUpper = false, hasDigit = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsLower(c)) hasLower = true;
            else if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
        }

        return hasLower && hasUpper && hasDigit && hasSpecial;
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string Username;
        public string Password;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
    }
}
