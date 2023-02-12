// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
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

Console.WriteLine("Start Chat");
Task.Run(ReadFromChat);
while (true)
{
    var text = Console.ReadLine();
    if (text == "exit")
    {
        break;
    }

    var message = new Message() {RoomName = room, Text = text, UserName = user};
    writer.Write(message.ToString());
}

client.Close();

async Task ReadFromChat()
{
    while (!tokenSource.IsCancellationRequested)
    {
        Message message = Message.Deserialize(reader.ReadString());
        Console.WriteLine($"{message.Time}|{message.UserName}: {message.Text}");
    }
}