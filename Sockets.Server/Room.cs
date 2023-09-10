using System.Collections.Concurrent;

namespace Sockets;

public static class RoomFactory
{
    public static ConcurrentDictionary<string, Room> CurrentRooms = new();

    public static object lock_object = new();

    public static Room CreateRoom(string Name)
    {
        return CurrentRooms.GetOrAdd(Name, s => new Room { Name = s });
    }


    public static void StopAll()
    {
        foreach (var (name, room) in CurrentRooms) room.DisconnectAll();
    }

    public static void PrintStats()
    {
        Console.WriteLine($"******* {DateTime.Now}");
        if (CurrentRooms.Count == 0) Console.WriteLine("No rooms!");

        foreach ((var name, var room) in CurrentRooms)
        {
            Console.WriteLine($"== {room.Name} - {room.clients.Count} ==");
            foreach (var client in room.clients) Console.WriteLine($"{client.UserName}: {client.RemoteEndPoint}");
            Console.WriteLine("Files:");
            foreach (var uploaded in room.FileMessages)
                Console.WriteLine(
                    $"{uploaded.FileServer} - {uploaded.FileInfo.Name} ({uploaded.FileSize.GetReadableFileSize()}) - {uploaded.FileLink}");
        }

        Console.WriteLine("*******");
    }

    public static async Task PrintStatsWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            PrintStats();
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    public static async Task CloseEmptyRoomsWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var room in CurrentRooms)
                if (room.Value.clients.Count == 0)
                    if (CurrentRooms.Remove(room.Key, out var tmp))
                        Console.WriteLine($"{room.Key} closed!");
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

public class Room
{
    public List<FileMessage> FileMessages = new();
    public object lock_object = new();
    public string Name { get; set; }
    public List<Client> clients { get; set; } = new();

    public async Task<Client> AddClient(Client client)
    {
        lock (lock_object)
        {
            clients.Add(client);
            client.room = this;
        }

        var hello_msg = new Message
        {
            Type = MessageType.System,
            RoomName = Name, Text = $"{DateTime.Now}: User {client.UserName} connected!",
            UserName = "SYSTEM"
        };
        await SendToAll(hello_msg, client);

        Console.WriteLine($"{DateTime.Now}: {Name}: {client.UserName} connected");
        return client;
    }

    public async Task RemoveClient(Client client)
    {
        lock (lock_object)
        {
            clients.Remove(client);
        }

        await SendToAll(Message.ByeMessage(Name, client.UserName));
    }


    public async Task SendToAll(Message message, Client? client = null)
    {
        if (client is null)
            foreach (var _client in clients)
                await _client.WriteMessage(message);
        else
            foreach (var _client in clients.Where(tcpClient => tcpClient != client))
                await _client.WriteMessage(message);
    }

    public async Task DisconnectAll()
    {
        await SendToAll(new Message
            { Type = MessageType.System, UserName = "", Text = "Room is closed", RoomName = Name });
        foreach (var client in clients) await client.Close();
    }
}