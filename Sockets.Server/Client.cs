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
                if (msg.Text == "CLIENT_BYE")
                {
                }

                continue;
            }

            await room.SendToAll(msg, this);
        }

        await Close();
    }


    public async Task Close()
    {
        room.clients.Remove(this);
        await room.SendToAll(new Message
        {
            Type = MessageType.System,
            Text = $"User {UserName} disconnected!",
            RoomName = room.Name
        });
        Connection.Close();
    }
}