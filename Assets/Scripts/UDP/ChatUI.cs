using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public ScrollRect scrollRect;
    public int maxMessages = 100;
    public TMP_InputField inputField;
    public TextMeshProUGUI messageText;
    private List<string> messages = new List<string>();

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    private GameObject serverInstance;
    private GameObject clientInstance;

    // Flag para saber si estamos actuando como servidor
    private bool isServer = false;

    public void StartServer()
    {
        if (serverInstance == null)
        {
            serverInstance = Instantiate(serverPrefab);

            // Asignar referencia al ChatUI en el servidor
            var server = serverInstance.GetComponent<Server>();
            if (server != null)
            {
                server.chatUI = this;
            }

            isServer = true;
            Debug.Log("Servidor iniciado.");
        }
    }

    public void StartClient()
    {
        if (clientInstance == null)
        {
            clientInstance = Instantiate(clientPrefab);

            var client = clientInstance.GetComponent<Client>();
            if (client != null)
            {
                client.chatUI = this;
            }

            Debug.Log("Cliente iniciado.");
            AppendMessage("Cliente iniciado. Conectando al servidor...");
        }
    }

    public void SendButton()
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;

        if (isServer && serverInstance != null)
        {
            // Si somos servidor, enviamos un mensaje como servidor
            var server = serverInstance.GetComponent<Server>();
            if (server != null)
            {
                server.SendServerMessage(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }
        else if (clientInstance != null)
        {
            // Si somos cliente, enviamos un mensaje como cliente
            var client = clientInstance.GetComponent<Client>();
            if (client != null)
            {
                client.SendChat(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }
        else
        {
            AppendMessage("Error: Ni servidor ni cliente iniciados.");
        }
    }

    public void AppendMessage(string message)
    {
        AppendMessage(message, Color.white);
    }

    public void AppendMessage(string message, Color color)
    {
        messages.Add($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");

        // Limitar el número de mensajes
        while (messages.Count > maxMessages)
        {
            messages.RemoveAt(0);
        }

        UpdateChatText();
    }

    private void UpdateChatText()
    {
        messageText.text = string.Join("\n", messages);

        // Desplazarse hacia abajo después de agregar un mensaje
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearChat()
    {
        messages.Clear();
        messageText.text = "";
    }

    public void ReturnMenu()
    {
        SceneManager.LoadScene(1);
    }
}