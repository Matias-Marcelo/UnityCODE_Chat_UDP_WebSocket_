using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField userRegister;
    [SerializeField] private TMP_InputField emailRegister;
    [SerializeField] private TMP_InputField emailLogin;
    [SerializeField] private TMP_Text errorLogin;
    [SerializeField] private TMP_Text errorRegister;
    [SerializeField] private GameObject canvasLogin;
    [SerializeField] private GameObject canvasRegister;

    private string postUrl = "https://madaso.mypressonline.com/api/Unitypost.php";

    void Start()
    {
        ShowLogin();
    }

    class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // ❗Ignora cualquier error SSL. Solo para pruebas.
        }
    }


    public void ShowRegister()
    {
        canvasLogin.SetActive(false);
        canvasRegister.SetActive(true);
    }

    public void ShowLogin()
    {
        canvasLogin.SetActive(true);
        canvasRegister.SetActive(false);
    }

    public void SendRegister()
    {
        // Get and clean up input values
        string username = userRegister.text.Trim();
        string email = emailRegister.text.Trim();

        // Validate required fields
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
        {
            errorRegister.SetText("The username and email are mandatory");
            return;
        }

        // Send registration request to server
        SendRequest(email, username, "register");
    }

    public void SendLogin()
    {
        // Get and clean up email input
        string email = emailLogin.text.Trim();

        // Validate required field
        if (string.IsNullOrEmpty(email))
        {
            errorLogin.SetText("Email is mandatory");
            return;
        }

        // Send login request to server (username not needed for login)
        SendRequest(email, "", "login");
    }

    void SendRequest(string email, string username, string action)
    {
        // Create request data object and convert to JSON
        RequestData data = new RequestData { email = email, username = username, action = action };
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(PostRequest(jsonData));
    }

    IEnumerator PostRequest(string jsonData)
    {
        // Create and configure POST request
        using (UnityWebRequest webRequest = new UnityWebRequest(postUrl, "POST"))

        {
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // ⚠️ Saltar verificación SSL solo para pruebas
            webRequest.certificateHandler = new BypassCertificate();
            webRequest.disposeCertificateHandlerOnDispose = true;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Parse server response
                string responseText = webRequest.downloadHandler.text;
                ResponseData response = JsonUtility.FromJson<ResponseData>(responseText);

                if (response.status == 201)
                {
                    // Registration successful, show login screen
                    ShowLogin();
                }
                else if (response.status == 200)
                {
                    // Login successful, save user data to PlayerPrefs
                    PlayerPrefs.SetInt("playerId", response.user.id);
                    PlayerPrefs.SetString("username", response.user.nombre);
                    PlayerPrefs.Save();

                    // Navigate to main menu scene
                    SceneManager.LoadScene(1);
                }
                else
                {
                    // Show error message from server
                    errorLogin.SetText(response.error);
                }
            }
            else
            {
                // Connection error
                errorLogin.SetText("There has been a problem with the connection to the server.");
                errorRegister.SetText("There has been a problem with the connection to the server.");
            }
        }
    }


    // Data send to server PHP
    [System.Serializable]
    public class RequestData
    {
        public string email;
        public string username;
        public string action;
    }

    // Class user
    [System.Serializable]
    public class User
    {
        public int id;
        public string nombre;
        public string email;
    }
    // Response of the server
    [System.Serializable]
    public class ResponseData
    {
        public string message;  // Success message
        public string error;    // Error message
        public int status;      // Status code (200: success, 201: created, etc.)
        public User user;       // User data object
    }
}