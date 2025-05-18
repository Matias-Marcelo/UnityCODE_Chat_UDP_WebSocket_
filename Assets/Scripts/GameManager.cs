using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private CanvasManager canvasManager;
    private PlayerController playerController;
    private float playTime = 0f;
    private bool isFirstStart = true;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find references in the new scene
        canvasManager = FindObjectOfType<CanvasManager>();
        playerController = FindObjectOfType<PlayerController>();

        // Update UI if we are in a game scene
        if (canvasManager != null)
        {
            canvasManager.UpdateDiamondCollect(PlayerStats.Instance.GetTotalPointsActuality());
            PlayerStats.Instance.SetCurrentLevel(scene.name);
        }
    }

    private void Start()
    {
        // Only set these values the first time the GameManager is started
        if (isFirstStart)
        {
            PlayerStats.Instance.SetCurrentLevel(SceneManager.GetActiveScene().name);
            PlayerStats.Instance.SetTotalPointsActuality(0);
            isFirstStart = false;
        }
    }

    private void Update()
    {
        // Increment play time
        playTime += Time.deltaTime;
    }

    public int GetPlayTimeInMinutes()
    {
        return Mathf.FloorToInt(playTime / 60f);
    }

    public void GetItemBig()
    {
        int totalItemCollect = PlayerStats.Instance.AddPointsActuality(5);
        if (canvasManager == null)
        {
            canvasManager = FindObjectOfType<CanvasManager>();
        }
        if (canvasManager != null)
        {
            canvasManager.UpdateDiamondCollect(totalItemCollect);
        }
    }

    public void SavePlayer(string slotName)
    {
        int points = PlayerStats.Instance.GetTotalPointsActuality();
        PlayerStats.Instance.AddTotalsPoints(points);
        PlayerStats.Instance.SetTotalPointsActuality(0);
        int timeGame = GetPlayTimeInMinutes();
        Debug.Log("Game time saved: " + timeGame + " minutes");

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        if (playerController != null)
        {
            float[] position = {
                playerController.transform.position.x,
                playerController.transform.position.y,
                playerController.transform.position.z
            };
            DataToSave data = new DataToSave(PlayerStats.Instance, position, timeGame);
            StartCoroutine(PostSaveData(JsonUtility.ToJson(data)));
        }
    }

    private IEnumerator PostSaveData(string jsonData)
    {
        string postUrl = "http://localhost/api/Unitypost.php";
        using (UnityWebRequest webRequest = new UnityWebRequest(postUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data saved successfully");
            }
            else
            {
                Debug.LogError("Error saving data: " + webRequest.error);
            }
        }
    }
}
