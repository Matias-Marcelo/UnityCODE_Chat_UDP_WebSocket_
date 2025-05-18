using Unity.Collections;

public enum MessageType : byte
{
    Chat,
    Command,
    Register,
    UserList,
    PrivateMessage,
    SystemMessage,
}

public struct ChatMessage
{
    public MessageType Type;
    public FixedString64Bytes Sender;
    public FixedString128Bytes Content;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Type);
        writer.WriteFixedString64(Sender);
        writer.WriteFixedString128(Content);
    }

    public static ChatMessage Deserialize(ref DataStreamReader reader)
    {
        ChatMessage msg;
        msg.Type = (MessageType)reader.ReadByte();
        msg.Sender = reader.ReadFixedString64();
        msg.Content = reader.ReadFixedString128();
        return msg;
    }
}
