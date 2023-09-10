using System.Text;

namespace Sockets;

public enum MessageType
{
    System,
    Text
}

// Протокол
// <MessageType><US><Комната><US><Имя Пользователя><STX><Сообщение><EOT>
public struct Message
{
    public static Message HelloMessage(string room, string user)
    {
        return new Message { RoomName = room, Type = MessageType.System, UserName = user, Text = "CLIENT_HELLO" };
    }

    public static Message ByeMessage(string room, string user)
    {
        return new Message { RoomName = room, Type = MessageType.System, UserName = user, Text = "CLIENT_BYE" };
    }

    public static Message ChangeRoom(string room, string user)
    {
        return new Message { RoomName = room, Type = MessageType.System, UserName = user, Text = "CHANGE_ROOM" };
    }


    public Message()
    {
    }

    private const char STX = (char)0x02;
    private const char EOT = (char)0x04;
    private const char US = (char)0x1F;

    public string RoomName { get; set; } = "General";
    public DateTime Time { get; set; } = DateTime.Now;
    public MessageType Type { get; set; }
    public string UserName { get; set; } = "";
    public string Text { get; set; } = "";

    public static Message Deserialize(byte[] msg)
    {
        return Deserialize(Encoding.UTF8.GetString(msg));
    }

    public static Message Deserialize(string msg)
    {
        var s = msg.TrimEnd(EOT).Split(STX);
        var head = s[0].Split(US);
        var res = new Message
        {
            Type = (MessageType)int.Parse(head[0]),
            Text = s[1],
            RoomName = head[1],
            UserName = head[2]
        };
        return res;
    }

    public byte[] Serialiaze()
    {
        return Encoding.UTF8.GetBytes(ToString());
    }

    public string ToString()
    {
        return ((int)Type).ToString() + US + RoomName + US + UserName + STX + Text + EOT;
    }
}