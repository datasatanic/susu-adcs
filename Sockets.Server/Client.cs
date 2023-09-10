using System.Net;
using System.Net.Sockets;

namespace Sockets;

public class Client
{
    public Client(TcpClient connection)
    {
        var stream = connection?.GetStream();
        Connection = connection;
        Reader = new BinaryReader(stream);
        Writer = new BinaryWriter(stream);
    }

    public Room room { get; set; }
    public string UserName { get; set; } = "unknown";

    public EndPoint RemoteEndPoint => Connection.Client.RemoteEndPoint;

    private TcpClient Connection { get; }
    private BinaryReader Reader { get; }
    private BinaryWriter Writer { get; }
    public bool DataAvailable => Connection.GetStream().DataAvailable;

    public async Task WriteMessage(Message message)
    {
        try
        {
            Writer.Write(message.ToString());
        }
        catch (IOException e)
        {
            Console.WriteLine($"Client {RemoteEndPoint} died. Closing");
            await Close();
        }
    }

    public Message ReadMessage()
    {
        var s = Reader.ReadString();
        return Message.Deserialize(s);
    }

    public async Task Serve(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var msg = Message.Deserialize(Reader.ReadString());

            if (msg.Type == MessageType.System)
            {
                switch (msg.Text)
                {
                    case "CLIENT_BYE":
                        break;
                    case "CHANGE_ROOM":
                        if (msg.RoomName == room.Name) continue;
                        await room.RemoveClient(this);
                        await RoomFactory.CreateRoom(msg.RoomName).AddClient(this);
                        break;
                }

                continue;
            }

            if (msg.Type == MessageType.FileUpload)
            {
                var message = FileMessage.GetFromMessage(msg);
                room.FileMessages.Add(message);
                await room.SendToAll(new Message
                {
                    RoomName = room.Name,
                    Type = MessageType.System,
                    UserName = UserName,
                    Text =
                        $"User - {UserName} upload file: {message.FileInfo.Name} {message.FileSize.GetReadableFileSize()} - {message.FileLink}"
                });
                continue;
            }

            if (msg.Type == MessageType.FileLoad)
            {
                var fileMessage = room.FileMessages.FirstOrDefault(message => message.FileLink == msg.Text, null);
                if (fileMessage is null) continue;

                fileMessage.Type = MessageType.FileLoad;
                await WriteMessage(fileMessage);
                continue;
            }

            await room.SendToAll(msg, this);
        }

        await Close();
    }


    public async Task Close()
    {
        room.clients.Remove(this);
        room.FileMessages.RemoveAll(message => message.UserName == UserName);
        await room.SendToAll(new Message
        {
            Type = MessageType.System,
            Text = $"User {UserName} disconnected!",
            RoomName = room.Name
        });
        Connection.Close();
    }
}

public static class ClientExtenstions
{
    private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static string GetReadableFileSize(this long size) // Size in bytes
    {
        var unitIndex = 0;
        while (size >= 1024)
        {
            size /= 1024;
            ++unitIndex;
        }

        var unit = Units[unitIndex];
        return string.Format("{0:0.#} {1}", size, unit);
    }
}