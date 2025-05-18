using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    public ChatUI chatUI;
    private bool isConnected = false;

    private NetworkDriver driver;
    private NetworkConnection connection;

    public string playerName;

    void Start()
    {
        playerName = PlayerPrefs.GetString("username");

        driver = NetworkDriver.Create();
        connection = driver.Connect(NetworkEndpoint.LoopbackIpv4.WithPort(9000));
    }

    void Update()
    {
        driver.ScheduleUpdate().Complete();

        if (!connection.IsCreated)
        {
            if (isConnected)
            {
                isConnected = false;
                chatUI.AppendMessage("Conexión perdida con el servidor.");
            }
            return;
        }

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out var stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                isConnected = true;
                SendRegister();
                chatUI.AppendMessage("Conectado al servidor.");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var msg = ChatMessage.Deserialize(ref stream);
                ProcessIncomingMessage(msg);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                isConnected = false;
                chatUI.AppendMessage("Desconectado del servidor.");
                connection = default;
            }
        }
    }

    private void ProcessIncomingMessage(ChatMessage msg)
    {
        switch (msg.Type)
        {
            case MessageType.Chat:
                chatUI.AppendMessage($"{msg.Sender}: {msg.Content}");
                break;
            case MessageType.SystemMessage:
                chatUI.AppendMessage($"Sistema: {msg.Content}", Color.yellow);
                break;
            case MessageType.UserList:
                chatUI.AppendMessage($"Usuarios conectados:\n{msg.Content}", Color.green);
                break;
            default:
                chatUI.AppendMessage($"{msg.Sender}: {msg.Content}");
                break;
        }
    }

    public void SendChat(string input)
    {
        if (!isConnected)
        {
            chatUI.AppendMessage("Error: No estás conectado al servidor.");
            return;
        }

        // Maneja comandos localmente
        if (input.StartsWith("/"))
        {
            if (CommandHandler.ProcessClientCommand(input, this))
            {
                return;
            }
        }
        // Si no es un comando local o el comando necesita ser enviado al servidor
        ChatMessage msg = new ChatMessage
        {
            Type = input.StartsWith("/") ? MessageType.Command : MessageType.Chat,
            Sender = new FixedString64Bytes(playerName),
            Content = new FixedString128Bytes(input)
        };
        SendMessage(msg);
    }


    private void SendMessage(ChatMessage msg)
    {
        if (driver.BeginSend(NetworkPipeline.Null, connection, out var writer) == 0)
        {
            msg.Serialize(ref writer);
            driver.EndSend(writer);
        }
        else
        {
            chatUI.AppendMessage("Error: No se pudo enviar el mensaje.", Color.red);
        }
    }

    private void SendRegister()
    {
        ChatMessage msg = new ChatMessage
        {
            Type = MessageType.Register,
            Sender = new FixedString64Bytes(playerName),
            Content = new FixedString128Bytes("Registrarse")
        };

        SendMessage(msg);
    }

    public void ChangePlayerName(string newName)
    {
        playerName = newName;
        SendRegister();
        chatUI.AppendMessage($"Nombre cambiado a: {playerName}", Color.green);
    }

    void OnDestroy()
    {
        if (connection.IsCreated)
        {
            connection.Disconnect(driver);
            driver.ScheduleUpdate().Complete();
        }
        driver.Dispose();
    }
}