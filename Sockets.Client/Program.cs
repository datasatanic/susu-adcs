// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using Sockets;
using Sockets.Client;

string[] good_ans = { "yes", "y", "ok" };

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
var fileServer = new FileServer();
fileServer.Start();
Console.WriteLine($"\rFileServer on: {fileServer.EndPoint}");
Task.Run(() => fileServer.Serve(tokenSource.Token));
while (!tokenSource.IsCancellationRequested)
{
    Console.Write($"{room}: ");

    var text = Console.ReadLine();

    if (text == "CLOSE_CHAT")
    {
        break;
    }

    Message? message = null;
    if (text.StartsWith("CHANGE_ROOM"))
    {
        var new_room = text.Substring("CHANGE_ROOM".Length).TrimStart();
        new_room = string.IsNullOrEmpty(new_room) ? "General" : room;

        message = Message.ChangeRoom(new_room, user);
        room = new_room;
    }

    if (text.StartsWith("FILE_UPLOAD"))
    {
        var path = text.Substring("FILE_UPLOAD".Length).TrimStart();
        var tmp = FileMessage.Create(room, user, path, fileServer.EndPoint);
        if (tmp is null) continue;

        fileServer.Files.AddOrUpdate(tmp.FileLink, tmp.FileInfo, (s, info) => info);
        message = tmp;
    }

    if (text.StartsWith("FILE_LOAD"))
    {
        var file_id = text.Substring("FILE_LOAD".Length).TrimStart();
        message = Message.LoadFile(room, user, file_id);
    }

    message ??= new Message { Type = MessageType.Text, RoomName = room, Text = text, UserName = user };
    writer.Write(message.ToString());
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
            if (message.Type == MessageType.FileLoad)
            {
                await LoadFile(FileMessage.GetFromMessage(message));
                continue;
            }

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


async Task LoadFile(FileMessage message)
{
    Console.WriteLine("DOWNOLADING FILE");
}