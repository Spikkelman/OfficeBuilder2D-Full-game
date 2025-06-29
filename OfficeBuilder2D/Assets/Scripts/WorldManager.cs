using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class WorldManager : MonoBehaviour
{
    [Header("World Creation Panel")]
    public TMP_InputField worldNameInput;
    public Button createWorldButton;
    public TMP_Text creationFeedbackText;

    [Header("World Overview Panel")]
    public Transform overviewContentPanel;
    public GameObject worldButtonPrefab;

    private List<WorldData> userWorlds = new List<WorldData>();
    private string baseUrl = "http://localhost:5136/api/worlds";

    [System.Serializable]
    public class WorldData
    {
        public int id;
        public string worldName;
        public int userId;
    }

    [System.Serializable]
    public class CreateWorldRequest
    {
        public string worldName;
    }

    [System.Serializable]
    public class WorldListWrapper
    {
        public List<WorldData> worlds;
    }

    void Start()
    {
        creationFeedbackText.text = "";
        createWorldButton.onClick.RemoveAllListeners(); //prevents duplicates
        createWorldButton.onClick.AddListener(() =>
        {
            Debug.Log("Create button clicked.");
            StartCoroutine(CreateWorldRequestRoutine());
        });

        StartCoroutine(GetWorldsRoutine());
    }

    IEnumerator GetWorldsRoutine()
    {
        string token = PlayerPrefs.GetString("auth_token");
        if (string.IsNullOrEmpty(token))
        {
            creationFeedbackText.text = "Not logged in.";
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/overview");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        Debug.Log("GET /worlds/overview response code: " + request.responseCode);

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            string rawJson = request.downloadHandler.text;
            Debug.Log("Worlds JSON: " + rawJson);

            string wrappedJson = "{\"worlds\":" + rawJson + "}";
            WorldListWrapper wrapper = JsonUtility.FromJson<WorldListWrapper>(wrappedJson);
            userWorlds = wrapper.worlds ?? new List<WorldData>();

            PopulateOverview();
        }
        else
        {
            creationFeedbackText.text = "Failed to load worlds.";
            Debug.LogError("Worlds fetch failed: " + request.downloadHandler.text);
        }
    }

    IEnumerator CreateWorldRequestRoutine()
    {
        string nameInput = worldNameInput.text.Trim();
        if (string.IsNullOrEmpty(nameInput) || nameInput.Length > 25)
        {
            creationFeedbackText.text = "World name must be between 1 and 25 characters.";
            yield break;
        }

        string token = PlayerPrefs.GetString("auth_token");
        Debug.Log("Sending POST request to create world...");
        CreateWorldRequest payload = new CreateWorldRequest { worldName = nameInput };
        string jsonData = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(baseUrl + "/create", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        Debug.Log("POST /worlds/create response code: " + request.responseCode);

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            creationFeedbackText.text = "World created.";
            worldNameInput.text = "";
            yield return new WaitForSeconds(0.3f); // slight delay to ensure DB updates
            StartCoroutine(GetWorldsRoutine());
        }
        else
        {
            creationFeedbackText.text = "Creation failed: " + request.downloadHandler.text;
            Debug.LogError("CreateWorld Error: " + request.downloadHandler.text);
        }
    }

    public void DeleteWorld(WorldData worldToDelete)
    {
        StartCoroutine(DeleteWorldRoutine(worldToDelete));
    }

    IEnumerator DeleteWorldRoutine(WorldData world)
    {
        string token = PlayerPrefs.GetString("auth_token");
        UnityWebRequest request = UnityWebRequest.Delete(baseUrl + "/" + world.id);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            creationFeedbackText.text = "World deleted.";
            StartCoroutine(GetWorldsRoutine());
        }
        else
        {
            creationFeedbackText.text = "Deletion failed: " + request.downloadHandler.text;
            Debug.LogError("DeleteWorld Error: " + request.downloadHandler.text);
        }
    }

    void PopulateOverview()
    {
        foreach (Transform child in overviewContentPanel)
            Destroy(child.gameObject);

        Debug.Log("Populating " + userWorlds.Count + " worlds");

        foreach (var world in userWorlds)
        {
            GameObject newButton = Instantiate(worldButtonPrefab, overviewContentPanel);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = world.worldName;

            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                WorldData localCopy = world;
                btn.onClick.AddListener(() => OnWorldSelected(localCopy));
            }

            Transform deleteBtnTransform = newButton.transform.Find("DeleteButton");
            if (deleteBtnTransform != null)
            {
                Button deleteBtn = deleteBtnTransform.GetComponent<Button>();
                deleteBtn.onClick.AddListener(() => DeleteWorld(world));
            }
        }
    }

    void OnWorldSelected(WorldData world)
    {
        Debug.Log("Opening world: " + world.worldName);

         // Opslaan in static class
        SelectedWorld.WorldId = world.id;
        SelectedWorld.WorldName = world.worldName;

        // Scene wisselen
        SceneManager.LoadScene("WorldOverviewScene"); // Pas deze naam aan aan jouw scene
    }
}