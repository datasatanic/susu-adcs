namespace Sockets;

public class RoomFactory
{
    public static CancellationTokenSource TokenSource = new();

    public Room? this[string name] => CurrentRooms.FirstOrDefault(room => room.Name == name, null);

    public List<Room> CurrentRooms => new();

    public Task<Room> StartRoom(string Name)
    {
        var room = new Room { Name = Name };
        CurrentRooms.Add(room);
        Task.Run(() => room.Serve(TokenSource.Token));
        return Task.FromResult(room);
    }

    public void StopAll()
    {
        foreach (var room in CurrentRooms) room.Stop();
    }
}

public class Room
{
    public string Name { get; set; }
    public List<Client> clients { get; set; } = new();

    public bool IsCancelled { get; set; }


    public async Task AddUser(Client client, string user)
    {
        clients.Add(client);
        var hello_msg = new Message
        {
            Type = MessageType.System,
            RoomName = Name, Text = $"User {user} connected!",
            UserName = ""
        };
        await SendToAll(hello_msg, client);

        Console.WriteLine($"{Name}: {user} connected");
    }

    public async Task SendToAll(Message message, Client? client = null)
    {
        if (client is null)
            foreach (var _client in clients)
                _client.WriteMessage(message);
        else
            foreach (var _client in clients.Where(tcpClient => tcpClient != client))
                _client.WriteMessage(message);
    }

    public async Task Serve(CancellationToken token)
    {
        while (!token.IsCancellationRequested || !IsCancelled)
            foreach (var client in clients)
                if (client.DataAvailable)
                {
                    var message = client.ReadMessage();
                    await SendToAll(message, client);
                }

        await SendToAll(new Message
            { Type = MessageType.System, UserName = "", Text = "Room is closed", RoomName = Name });
        foreach (var client in clients) client.Close();
    }

    public async Task Stop()
    {
        IsCancelled = false;
    }
}