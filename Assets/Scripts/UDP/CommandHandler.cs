using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;

public static class CommandHandler
{
    public delegate void CommandAction(NetworkConnection sender, string[] args, Server server);
    public delegate void ClientCommandAction(string[] args, Client client);

    // Comandos del servidor
    private static Dictionary<string, CommandAction> serverCommands = new Dictionary<string, CommandAction>
    {
        { "ayuda", ShowServerHelp },
        { "lista", ListUsers },
    };

    // Comandos del cliente
    private static Dictionary<string, ClientCommandAction> clientCommands = new Dictionary<string, ClientCommandAction>
    {
        { "ayuda", ShowClientHelp },
        { "nombre", ChangeName },
        { "limpiar", ClearChat },
    };

    // Procesa un comando en el servidor
    public static bool ProcessServerCommand(NetworkConnection sender, string message, Server server)
    {
        if (!message.StartsWith("/"))
            return false;

        string[] parts = message.Substring(1).Split(' ');
        string command = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        if (serverCommands.TryGetValue(command, out CommandAction action))
        {
            action(sender, args, server);
            return true;
        }

        return false;
    }

    // Procesa un comando en el cliente
    public static bool ProcessClientCommand(string message, Client client)
    {
        if (!message.StartsWith("/"))
            return false;

        string[] parts = message.Substring(1).Split(' ');
        string command = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        if (clientCommands.TryGetValue(command, out ClientCommandAction action))
        {
            action(args, client);
            return true;
        }

        return false;
    }

    // Implementaciones de comandos del servidor
    private static void ShowServerHelp(NetworkConnection sender, string[] args, Server server)
    {
        string helpText = "Comandos disponibles para el servidor:\n" +
                          "/ayuda - Muestra esta ayuda\n" +
                          "/lista - Muestra los usuarios conectados\n";

        // Si es el servidor el que envía el comando (conexión por defecto)
        if (sender.Equals(default(NetworkConnection)))
        {
            server.chatUI.AppendMessage(helpText, UnityEngine.Color.green);
        }
        else
        {
            // Enviar ayuda solo al remitente si es un cliente
            server.SendPrivateMessage(sender, server.serverName, helpText);
        }
    }

    private static void ListUsers(NetworkConnection sender, string[] args, Server server)
    {
        var users = server.GetConnectedUsers();
        string usersList = "Usuarios conectados:\n" + string.Join("\n", users);

        // Si es el servidor el que envía el comando
        if (sender.Equals(default(NetworkConnection)))
        {
            server.chatUI.AppendMessage(usersList, UnityEngine.Color.green);
        }
        else
        {
            // Enviar la lista solo al remitente si es un cliente
            server.SendPrivateMessage(sender, server.serverName, usersList);
        }
    }

    // Implementaciones de comandos del cliente
    private static void ShowClientHelp(string[] args, Client client)
    {
        string helpText = "Comandos disponibles:\n" +
                          "/ayuda - Muestra esta ayuda\n" +
                          "/nombre <nuevo_nombre> - Cambia tu nombre de usuario\n" +
                          "/limpiar - Limpia el chat\n";

        client.chatUI.AppendMessage(helpText);
    }

    private static void ChangeName(string[] args, Client client)
    {
        if (args.Length < 1)
        {
            client.chatUI.AppendMessage("Uso: /nombre <nuevo_nombre>");
            return;
        }

        string newName = args[0];
        client.ChangePlayerName(newName);
    }

    private static void ClearChat(string[] args, Client client)
    {
        client.chatUI.ClearChat();
        client.chatUI.AppendMessage("Chat limpiado.");
    }
}