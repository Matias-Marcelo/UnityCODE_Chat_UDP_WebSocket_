using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitialMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text diamondCollect;
    [SerializeField] private GameObject canvasSelectLevel;
    [SerializeField] private GameObject canvasInitialMenu;
    [SerializeField] private GameObject canvasSelectSave;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private Button level2Button;
    [SerializeField] public GameObject rankingRowPrefab;

    public void StartNewGame()
    {
        SceneManager.LoadScene(2);
    }

    public void Start()
    {
        int points = PlayerStats.Instance.GetTotalPoints();
        diamondCollect.text = points.ToString();

        canvasSelectLevel.SetActive(false);
        canvasSelectSave.SetActive(false);
        rankingPanel.SetActive(false);

        CheckLevelUnlock(); // Check if level 2 is unlocked
    }

    public void ShowRanking()
    {
        // Create a request object to get ranking data from server
        RequestData data = new RequestData { action = "get_ranking" };
        StartCoroutine(GetRanking(JsonUtility.ToJson(data)));
    }

    private IEnumerator GetRanking(string jsonData)
    {
        // Send POST request to PHP API to get ranking data
        string postUrl = "http://localhost/api/Unitypost.php";
        using (UnityWebRequest webRequest = new UnityWebRequest(postUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                // Convert JSON response to RankingResponse object
                RankingResponse rankingResponse = JsonUtility.FromJson<RankingResponse>(response);
                ShowRankingInUI(rankingResponse.data);
            }
            else
            {
                Debug.LogError("Error saving data: " + webRequest.error);
            }
        }
    }

    public void ShowRankingInUI(List<RankingEntry> entries)
    {
        rankingPanel.SetActive(true);
        // Create a single container prefab for all ranking entries
        GameObject container = Instantiate(rankingRowPrefab, rankingPanel.transform);

        // Get references to the three TextMeshPro components that will display columns of data
        TMP_Text[] texts = container.GetComponentsInChildren<TMP_Text>();

        if (texts.Length >= 3)
        {
            // Use StringBuilder for efficient string concatenation
            StringBuilder nombres = new StringBuilder();
            StringBuilder puntuaciones = new StringBuilder();
            StringBuilder partidas = new StringBuilder();

            // Iterate through each entry and build text columns
            for (int i = 0; i < entries.Count; i++)
            {
                RankingEntry entry = entries[i];

                // Add each player's data to the appropriate column
                nombres.AppendLine(entry.nombre);
                puntuaciones.AppendLine(entry.mejor_puntuacion.ToString());
                partidas.AppendLine(entry.total_partidas.ToString());
            }

            // Assign the built strings to each column
            texts[0].text = nombres.ToString();
            texts[1].text = puntuaciones.ToString();
            texts[2].text = partidas.ToString();
        }
    }

    public void ReturnRanking()
    {
        rankingPanel.SetActive(false);
    }

    private void CheckLevelUnlock()
    {
        int totalPoints = PlayerStats.Instance.GetTotalPoints();

        // Enable level 2 button only if player has 20 or more points
        if (totalPoints >= 20)
        {
            level2Button.interactable = true; // Enable the button
        }
        else
        {
            level2Button.interactable = false; // Disable the button, but keep it visible
        }
    }


    public void SelectLevel()
    {
        canvasSelectLevel.SetActive(true);
        canvasInitialMenu.SetActive(false);
    }

    public void SelectInitialMenu()
    {
        canvasInitialMenu.SetActive(true);
        canvasSelectLevel.SetActive(false);
    }

    public void LoadLevelSelected(string name)
    {
        SceneManager.LoadScene(name);
    }


    public void ReturnToMenu()
    {
        canvasInitialMenu.SetActive(true);
        canvasSelectLevel.SetActive(false);
        canvasSelectSave.SetActive(false);
    }

    public void LoadGame()
    {
        canvasSelectSave.SetActive(true);
        canvasInitialMenu.SetActive(false);
    }

    public void LoadGameSlot(int slotNumber)
    {
        // Generate save slot name (SaveSlot1, SaveSlot2, etc.)
        string slotName = "SaveSlot" + slotNumber;

        // Load saved data from the selected slot
        DataToSave data = SaveManager.LoadDataPlayer(slotName);
        PlayerStats.Instance.SetCurrentLevel(data.currentLevel);
        PlayerStats.Instance.SetTotalPointsActuality(data.currentPoints);
        PlayerStats.Instance.SetTotalPoints(data.totalPoints);

        // Load the saved level
        SceneManager.LoadScene(data.currentLevel);
    }

    public void LoadUdp()
    {
        string name = "Udp";
        SceneManager.LoadScene(name);
    }

    public void LoadWebSocket()
    {
        string name = "Websocket";
        SceneManager.LoadScene(name);
    }


    // Data send to server
    [System.Serializable]
    public class RequestData
    {
        public string action;
    }

    // Data received from server
    [System.Serializable]
    public class RankingResponse
    {
        public int status;
        public string message;
        public List<RankingEntry> data;
    }

    [System.Serializable]
    public class RankingEntry
    {
        public string nombre;        // Player name
        public int mejor_puntuacion; // Best score
        public int total_partidas;   // Total games played
    }
}
