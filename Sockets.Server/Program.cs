using System.Net;
using System.Net.Sockets;
using Sockets;

int serverPort = 16666;
IPAddress serverAddr = IPAddress.Parse("0.0.0.0");


CancellationTokenSource tokenSource = new CancellationTokenSource();


Task.Run(() => StartServer(tokenSource.Token));

Console.ReadKey();

tokenSource.Cancel();


async Task StartServer(CancellationToken token)
{
    TcpListener listener = new TcpListener(serverAddr, serverPort);
    listener.Start();
    var rooms = new RoomFactory();
    Console.WriteLine("Server starting!");
    while (!token.IsCancellationRequested)
    {
        var TCPclient = await listener.AcceptTcpClientAsync();
        var client = new Client(TCPclient);
        var message = client.ReadMessage();
        var room = rooms[message.RoomName] ?? await rooms.StartRoom(message.RoomName);
        room.AddUser(client, message.UserName);
    }

    rooms.StopAll();
    listener.Stop();
    Console.WriteLine("Server stopped!");
}