// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using Sockets;

int serverPort = 16666;
string serverAddr = "localhost";
NAME:
Console.Write("What is your nickname?: ");
string user = Console.ReadLine();
if (string.IsNullOrEmpty(user)) goto NAME;

Console.Write("What room will you use?: ");
var room = Console.ReadLine();
room = string.IsNullOrEmpty(room) ? "General" : room;

TcpClient client = new TcpClient(serverAddr, serverPort);
NetworkStream stream = client.GetStream();
BinaryReader reader = new BinaryReader(stream);
BinaryWriter writer = new BinaryWriter(stream);

CancellationTokenSource tokenSource = new CancellationTokenSource();


Console.CancelKeyPress += (sender, eventArgs) => tokenSource.Cancel();


writer.Write(Message.HelloMessage(room, user).ToString());

Task.Run(ReadFromChat);
while (!tokenSource.IsCancellationRequested)
{
    Console.Write($"{room}: ");

    var text = Console.ReadLine();

    if (text == "CLOSE_CHAT")
    {
        break;
    }

    if (text.StartsWith("CHANGE_ROOM"))
    {
        var new_room = text.Substring("CHANGE_ROOM".Length).TrimStart();
        writer.Write(Message.ChangeRoom(new_room, user).ToString());
        room = new_room;
        continue;
    }

    var message = new Message { Type = MessageType.Text, RoomName = room, Text = text, UserName = user };
    writer.Write(message.ToString());
    writer.Flush();
}

try
{
    writer.Write(Message.ByeMessage(room, user).ToString());
}
catch (IOException e)
{
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
finally
{
    client.Close();
}

Console.WriteLine("Disconnected");

async Task ReadFromChat()
{
    while (!tokenSource.IsCancellationRequested)
    {
        try
        {
            var s = reader.ReadString();
            var message = Message.Deserialize(s);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Beep();
            Console.WriteLine("\r{0}|{3}|{1}: {2}", message.Time, message.UserName, message.Text, message.RoomName);
            Console.ResetColor();
            Console.Write($"{room}: ");
        }
        catch (IOException)
        {
            tokenSource.Cancel();
            break;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}