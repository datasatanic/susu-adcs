// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using Sockets;

int serverPort = 16666;
string serverAddr = "localhost";

Console.Write("What is your nickname?: ");
string user = Console.ReadLine();
Console.Write("What room will you use?: ");
string room = Console.ReadLine() ?? "General";

TcpClient client = new TcpClient(serverAddr, serverPort);
NetworkStream stream = client.GetStream();
BinaryReader reader = new BinaryReader(stream);
BinaryWriter writer = new BinaryWriter(stream);

CancellationTokenSource tokenSource = new CancellationTokenSource();

var hello_msg = new Message { RoomName = room, Type = MessageType.System, UserName = user, Text = "CLIENT_HELLO" };

writer.Write(hello_msg.ToString());

Task.Run(ReadFromChat);
while (true)
{
    var text = Console.ReadLine();
    if (text == "CLOSE_CHAT")
    {
        break;
    }

    var message = new Message { Type = MessageType.Text, RoomName = room, Text = text, UserName = user };
    writer.Write(message.ToString());
}

writer.Write(
    new Message { Type = MessageType.System, RoomName = room, Text = "CLIENT_BYE", UserName = user }.ToString());

client.Close();

async Task ReadFromChat()
{
    while (!tokenSource.IsCancellationRequested)
    {
        var s = reader.ReadString();
        var message = Message.Deserialize(s);
        Console.WriteLine($"{message.Time}|{message.UserName}: {message.Text}");
    }
}