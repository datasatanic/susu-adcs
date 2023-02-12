using System.Net;
using System.Net.Sockets;
using System.Text;
using Sockets;

int serverPort = 16666;
IPAddress serverAddr = IPAddress.Parse("0.0.0.0");


CancellationTokenSource tokenSource = new CancellationTokenSource();
RoomFactory roomFactory = new RoomFactory();

Task.Run(() => StartServer(tokenSource.Token));

Console.ReadKey();
tokenSource.Cancel();



async Task StartServer(CancellationToken token)
{
    TcpListener listener = new TcpListener(serverAddr, serverPort);
    listener.Start();
    Console.WriteLine("Server starting!");
    List<Room> CurrentRooms = new List<Room>(){new Room(){Name = "General"}};
    while (!token.IsCancellationRequested)
    {
        TcpClient client = listener.AcceptTcpClient();
        BinaryReader reader = new BinaryReader(client.GetStream(), Encoding.UTF8);
        var tmp=reader.ReadString();
        Message message = Message.Deserialize(tmp);
        
        var room = roomFactory[message.RoomName]?? await roomFactory.CreateRoom(message.RoomName, token);
        await room.UserConnected(client, message.UserName);
    }
    listener.Stop();
    Console.WriteLine("Server stopped!");
}


