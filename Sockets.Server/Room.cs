using System.Net.Sockets;
using System.Text;

namespace Sockets;

public class RoomFactory
{
    public List<Room> CurrentRooms = new();

    public Task<Room> CreateRoom(string Name, CancellationToken token)
    {
        Room room = new Room() {Name = Name, token = token};
        CurrentRooms.Add(room);
        Task.Run(room.Serve);
        Console.WriteLine($"Room: {Name} created");
        return Task.FromResult(room);
    }

    public Room? this[string name] => CurrentRooms.FirstOrDefault(room => room.Name == name, null);
}

public class Room
{
    public List<TcpClient> clients { get; set; } = new();

    public string Name { get; set; }
    public CancellationToken token { get; init; }

    public async Task UserConnected(TcpClient client, string user)
    {
        clients.Add(client);
        foreach (var _client in clients.Where(tcpClient => tcpClient != client))
        {
            Message hello_msg = new Message()
            {
                RoomName = Name, Text = $"User {user} connected!",
                UserName = ""
            };
            await _client.GetStream().WriteAsync(hello_msg.Serialiaze());
        }

        Console.WriteLine($"{Name}: {user} connected");
    }

    public async Task Serve()
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var client in clients)
            {
                var stream = client.GetStream();
                if (!stream.DataAvailable)
                {
                    continue;
                }

                BinaryReader reader = new BinaryReader(stream);
                var message = Message.Deserialize(reader.ReadString());
                foreach (var _client in clients.Where(tcpClient => tcpClient != client))
                {
                    await _client.GetStream().WriteAsync(message.Serialiaze());
                }
            }
        }
    }
}

// Протокол
// <Комната><US><Имя Пользователя><STX><Сообщение><EOT>
public struct Message
{
    public Message()
    {
    }

    private const char STX = (char) 0x02;
    private const char EOT = (char) 0x04;
    private const char US = (char) 0x1F;

    public string RoomName { get; set; } = "General";
    public DateTime Time { get; set; } = DateTime.Now;
    public string UserName { get; set; } = "";
    public string Text { get; set; } = "";

    public static Message Deserialize(byte[] msg) => Deserialize(Encoding.UTF8.GetString(msg));

    public static Message Deserialize(string msg)
    {
        string[] s = msg.TrimEnd(EOT).Split(STX);
        string[] head = s[0].Split(US);
        return new Message
        {
            Text = s[1],
            RoomName = head[0],
            UserName = head[1]
        };
    }

    public byte[] Serialiaze()
    {
        return Encoding.UTF8.GetBytes(ToString());
    }

    public string ToString()
    {
        return RoomName + US + UserName + STX + Text + EOT;
    }
}