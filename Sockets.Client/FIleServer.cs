using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Sockets.Client;

public class FileServer : IDisposable
{
    private static readonly TcpListener _listener = new(IPAddress.Any, 0);

    public ConcurrentDictionary<string, FileInfo> Files = new();

    public IPEndPoint EndPoint => (IPEndPoint)_listener.Server.LocalEndPoint;

    public void Dispose()
    {
        Stop();
    }

    public void Start()
    {
        _listener.Start();
    }

    public void Stop()
    {
        _listener.Stop();
    }


    public async Task Serve(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var client = await _listener.AcceptTcpClientAsync(token);
                Console.WriteLine($"Client Accepted {client.Client.RemoteEndPoint}");
                var reader = new BinaryReader(client.GetStream());
                var writer = new BinaryWriter(client.GetStream());
                var file_id = reader.ReadString();
                var file_guid = Files.GetValueOrDefault(file_id, null);
                Console.WriteLine($"Send {file_guid.Name}");
                if (file_guid is null) client.Close();
                writer.Write(File.ReadAllBytes(file_guid.FullName));
                client.Close();
                Console.WriteLine("Transmission ended!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Transmission failed!");
            }
        }
    }
}