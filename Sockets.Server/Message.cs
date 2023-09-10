using System.Net;
using System.Text;

namespace Sockets;

public enum MessageType
{
    System,
    Text,
    FileUpload,
    FileLoad
}

// Протокол
// <MessageType><US><Комната><US><Имя Пользователя><STX><Сообщение><EOT>
public class Message
{
    protected const char STX = (char)0x02;
    protected const char EOT = (char)0x04;
    protected const char US = (char)0x1F;


    public Message()
    {
    }

    public string RoomName { get; set; } = "General";
    public DateTime Time { get; set; } = DateTime.Now;
    public MessageType Type { get; set; }
    public string UserName { get; set; } = "";
    public virtual string Text { get; set; } = "";

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

    public static Message LoadFile(string room, string user, string file_id)
    {
        return new Message { RoomName = room, Type = MessageType.FileLoad, UserName = user, Text = file_id };
    }

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

public class FileMessage : Message
{
    public string FileLink = Guid.NewGuid().ToString();
    public FileInfo FileInfo { get; set; }
    public long FileSize { get; set; }

    public IPEndPoint FileServer { get; set; }

    public override string Text => FileServer.ToString() + US + FileInfo.Name + US + FileSize + US + FileLink;

    public static FileMessage? Create(string room, string user, string path, IPEndPoint serverEndpoint)
    {
        var _path = path.Trim();
        if (!File.Exists(_path))
        {
            Console.WriteLine("FILE NOT FOUND!");
            return null;
        }

        var fileInfo = new FileInfo(_path);

        return new FileMessage
        {
            RoomName = room,
            Type = MessageType.FileUpload,
            UserName = user,
            FileInfo = fileInfo,
            FileSize = fileInfo.Length,
            FileServer = serverEndpoint
        };
    }

    public static FileMessage GetFromMessage(Message msg)
    {
        var s = msg.Text.Split(US);
        return new FileMessage
        {
            FileServer = IPEndPoint.Parse(s[0]),
            FileInfo = new FileInfo(s[1]),
            FileSize = long.Parse(s[2]),
            FileLink = s[3],
            RoomName = msg.RoomName,
            UserName = msg.UserName,
            Time = msg.Time,
            Type = msg.Type
        };
    }

    public new static FileMessage Deserialize(string msg)
    {
        var s = msg.TrimEnd(EOT).Split(STX);
        var head = s[0].Split(US);
        var fileInfo = s[1].Split(US);
        var res = new FileMessage
        {
            Type = (MessageType)int.Parse(head[0]),
            FileServer = IPEndPoint.Parse(fileInfo[0]),
            FileInfo = new FileInfo(fileInfo[1]),
            FileSize = long.Parse(fileInfo[2]),
            FileLink = fileInfo[3],
            RoomName = head[1],
            UserName = head[2]
        };
        return res;
    }
}