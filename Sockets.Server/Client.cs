using System.Net.Sockets;

namespace Sockets;

public class Client
{
    public Client(TcpClient connection)
    {
        var stream = connection?.GetStream();
        Reader = new BinaryReader(stream);
        Writer = new BinaryWriter(stream);
    }

    private TcpClient connection { get; set; }
    private BinaryReader Reader { get; }
    private BinaryWriter Writer { get; }
    public bool DataAvailable => connection.GetStream().DataAvailable;

    public void WriteMessage(Message message)
    {
        Writer.Write(message.ToString());
    }

    public Message ReadMessage()
    {
        return Message.Deserialize(Reader.ReadString());
    }

    public void Close()
    {
        connection.Close();
    }
}