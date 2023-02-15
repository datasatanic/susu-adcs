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
        if (CurrentRooms.Count == 0) Console.WriteLine("No rooms!");

        foreach ((var name, var room) in CurrentRooms)
        {
            Console.WriteLine($"{room.Name}: {room.clients.Count}");
            foreach (var client in room.clients) Console.WriteLine($"\t{client.RemoteEndPoint}");
        }
    }

    public static async Task PrintStatsWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            PrintStats();
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

public class Room
{
    public object lock_object = new();
    public string Name { get; set; }
    public List<Client> clients { get; set; } = new();

    public async Task<Client> AddClient(Client client, string user)
    {
        lock (lock_object)
        {
            clients.Add(client);
            client.room = this;
        }

        var hello_msg = new Message
        {
            Type = MessageType.System,
            RoomName = Name, Text = $"User {user} connected!",
            UserName = ""
        };
        await SendToAll(hello_msg, client);

        Console.WriteLine($"{Name}: {user} connected");
        return client;
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

    public async Task DisconnectAll()
    {
        await SendToAll(new Message
            { Type = MessageType.System, UserName = "", Text = "Room is closed", RoomName = Name });
        foreach (var client in clients) client.Close();
    }


    // public async Task Serve(CancellationToken token)
    // {
    //     while (!token.IsCancellationRequested || !IsCancelled)
    //         foreach (var client in clients)
    //         {
    //             if (client.DataAvailable)
    //             {
    //                 var message = client.ReadMessage();
    //                 if (message.Type == MessageType.System)
    //                 {
    //                     if (message.Text == "CLIENT_BYE")
    //                     {
    //                         client.Close();
    //                         clients.Remove(client);
    //                         await SendToAll(new Message()
    //                         {
    //                             Type = MessageType.System, Text = $"User {message.UserName} disconnected!",
    //                             RoomName = Name
    //                         });
    //                     }
    //
    //                     continue;
    //                 }
    //
    //                 await SendToAll(message, client);
    //             }
    //             Console.WriteLine("{0}{1}{2} No data",DateTime.Now,client.RemoteEndPoint);
    //         }
    //
    //     await SendToAll(new Message
    //         { Type = MessageType.System, UserName = "", Text = "Room is closed", RoomName = Name });
    //     foreach (var client in clients) client.Close();
    // }

    // public async Task Stop()
    // {
    //     IsCancelled = false;
    // }
}