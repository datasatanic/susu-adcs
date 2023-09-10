﻿using System.Net;
using System.Net.Sockets;
using Sockets;

int serverPort = 16666;
IPAddress serverAddr = IPAddress.Parse("0.0.0.0");


CancellationTokenSource tokenSource = new CancellationTokenSource();


Task.Run(() => StartServer(tokenSource.Token));


Console.ReadLine();
tokenSource.Cancel();

Console.WriteLine("Program closed!");


async Task StartServer(CancellationToken token)
{
    TcpListener listener = new TcpListener(serverAddr, serverPort);
    listener.Start();
    Console.WriteLine("Server starting!");
    Task.Run(() => RoomFactory.PrintStatsWorker(token));
    try
    {
        while (!token.IsCancellationRequested)
        {
            var TCPclient = await listener.AcceptTcpClientAsync(token);
            Console.WriteLine($"Client Accepted {TCPclient.Client.RemoteEndPoint}");
            var client = new Client(TCPclient);
            var message = client.ReadMessage();
            client.UserName = message.UserName;
            Task.Run(() => RoomFactory.CreateRoom(message.RoomName))
                .ContinueWith(async task => task.Result.AddClient(client))
                .ContinueWith(task => client.Serve(token));
        }
    }
    finally
    {
        RoomFactory.StopAll();
        listener.Stop();
        Console.WriteLine("Server stopped!");
    }
}