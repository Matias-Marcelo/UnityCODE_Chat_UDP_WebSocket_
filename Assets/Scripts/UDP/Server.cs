using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private Dictionary<NetworkConnection, FixedString64Bytes> playerNames;
    private Dictionary<FixedString64Bytes, NetworkConnection> nameToConnection;

    // Referencia al ChatUI
    public ChatUI chatUI;
    public string serverName;

    void Start()
    {
        serverName = PlayerPrefs.GetString("username");

        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(9000);
        if (driver.Bind(endpoint) != 0)
        {
            Debug.LogError("No se pudo vincular al puerto 9000.");
        }
        else
        {
            driver.Listen();
            if (chatUI != null)
                chatUI.AppendMessage("Servidor Iniciado");
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        playerNames = new Dictionary<NetworkConnection, FixedString64Bytes>();
        nameToConnection = new Dictionary<FixedString64Bytes, NetworkConnection>();
    }

    void Update()
    {
        driver.ScheduleUpdate().Complete();

        // Aceptar nuevas conexiones
        NetworkConnection c;
        while ((c = driver.Accept()) != default)
        {
            connections.Add(c);
            Debug.Log("Cliente conectado.");
            if (chatUI != null)
                chatUI.AppendMessage("Nuevo cliente conectado.");
        }

        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out var stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    ChatMessage msg = ChatMessage.Deserialize(ref stream);
                    HandleMessage(connections[i], msg);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Cliente desconectado.");

                    string playerName = "";
                    if (playerNames.ContainsKey(connections[i]))
                    {
                        playerName = playerNames[connections[i]].ToString();
                        nameToConnection.Remove(playerNames[connections[i]]);
                        playerNames.Remove(connections[i]);
                    }

                    if (chatUI != null)
                        chatUI.AppendMessage(string.IsNullOrEmpty(playerName) ?
                            "Cliente desconectado." :
                            $"Cliente {playerName} desconectado.");

                    connections[i] = default;
                }
            }
        }
    }

    private void HandleMessage(NetworkConnection sender, ChatMessage msg)
    {
        switch (msg.Type)
        {
            case MessageType.Register:
                // Registrar el nuevo nombre
                playerNames[sender] = msg.Sender;
                nameToConnection[msg.Sender] = sender;

                Debug.Log($"Registrado: {msg.Sender}");
                SendSystemMessage($"Usuario {msg.Sender} se ha conectado.");
                break;

            case MessageType.Chat:
                Debug.Log($"{msg.Sender}: {msg.Content}");

                if (chatUI != null)
                    chatUI.AppendMessage($"{msg.Sender}: {msg.Content}");
                BroadcastMessage(msg);
                break;

            case MessageType.Command:
                string commandText = msg.Content.ToString();
                if (!CommandHandler.ProcessServerCommand(sender, commandText, this))
                {
                    // Si no es un comando válido, tratarlo como mensaje normal
                    Debug.Log($"{msg.Sender} tried command: {msg.Content}");
                    if (chatUI != null)
                        chatUI.AppendMessage($"{msg.Sender} intentó usar comando: {msg.Content}");
                }
                break;
        }
    }

    private void BroadcastMessage(ChatMessage msg)
    {
        foreach (var conn in connections)
        {
            if (!conn.IsCreated) continue;

            if (driver.BeginSend(NetworkPipeline.Null, conn, out var writer) == 0)
            {
                msg.Serialize(ref writer);
                driver.EndSend(writer);
            }
            else
            {
                Debug.LogWarning("No se pudo iniciar el envío al cliente.");
            }
        }
    }

    public void SendPrivateMessage(NetworkConnection recipient, string senderName, string content)
    {
        ChatMessage msg = new ChatMessage
        {
            Type = MessageType.PrivateMessage,
            Sender = new FixedString64Bytes(senderName),
            Content = new FixedString128Bytes(content)
        };

        if (driver.BeginSend(NetworkPipeline.Null, recipient, out var writer) == 0)
        {
            msg.Serialize(ref writer);
            driver.EndSend(writer);
        }
    }

    // Método para que el servidor envíe mensajes
    public void SendServerMessage(string content)
    {
        // Comprobar si es un comando
        if (content.StartsWith("/"))
        {
            // Procesar comandos locales del servidor
            if (ProcessLocalServerCommand(content))
            {
                return; // Si el comando se procesó correctamente, no enviamos mensaje
            }
        }

        // Si no es un comando o el comando no se procesó, enviar como mensaje normal
        ChatMessage msg = new ChatMessage
        {
            Type = MessageType.Chat,
            Sender = new FixedString64Bytes(serverName),
            Content = new FixedString128Bytes(content)
        };

        if (chatUI != null)
            chatUI.AppendMessage($"{serverName}: {content}");

        BroadcastMessage(msg);
    }

    // Método para procesar comandos locales del servidor
    private bool ProcessLocalServerCommand(string command)
    {
        NetworkConnection dummyConnection = default;
        return CommandHandler.ProcessServerCommand(dummyConnection, command, this);
    }

    public void SendSystemMessage(string content)
    {
        ChatMessage msg = new ChatMessage
        {
            Type = MessageType.SystemMessage,
            Sender = new FixedString64Bytes(serverName),
            Content = new FixedString128Bytes(content)
        };

        if (chatUI != null)
            chatUI.AppendMessage($"Sistema: {content}", Color.yellow);

        BroadcastMessage(msg);
    }

    public List<string> GetConnectedUsers()
    {
        return playerNames.Values.Select(name => name.ToString()).ToList();
    }

    void OnDestroy()
    {
        driver.Dispose();
        connections.Dispose();
    }
}