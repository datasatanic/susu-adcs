# Архитектура распределенных вычеслительных систем

## Lab 1: Sockets

Projects: Sockets.Client, Sockets.Server

### Sockets.Server

Requirements: dotnet-sdk:7.*

Start:
```
dotnet run --project Sockets.Server <IP address> <Port>
```
* Optional ```<IP address>``` default ```0.0.0.0```
* Optional ``<Port>`` default ``16666``

Stats message format:
```
*** <Current Time>
== <Room Name> - <Users in room> ==
<user name>: <remote ip>:<remote port>
<user name>: <remote ip>:<remote port>
<user name>: <remote ip>:<remote port>
...
Files:
<Client upload endpoint> - <File name> - <File id>
<Client upload endpoint> - <File name> - <File id>
<Client upload endpoint> - <File name> - <File id>
```
### Sockets.Server

Requirements: dotnet-sdk:7.*

Start:
```
dotnet run --project Sockets.Client <Server IP address> <Server Port>
```
* Optional ```<Server IP address>``` default ```0.0.0.0```
* Optional ``<Server Port>`` default ``16666``

Client Commands:
- ```<Message Text>``` - send message text to all users in room
- ```CLOSE_CHAT``` - close program
- ```CHANGE_ROOM <Room name>``` - change room (with exit from current)
- ```FILE_UPLOAD <file path>``` - tell others speakers that you can send file via link (Others recieve message like ``<user name> upload file: <file name> <file size> - <file_link>``)
- ```FILE_LOAD <file_link>``` - download file from link

## LAb 2: Serialization formats

Projects: SerializationFormats

Requirements: dotnet-sdk:7.*

Start:
```
dotnet run -c Release --project SerializationFormats/SerializationFormats.csproj -- --join -f '*'
```
> Results in BenchmarkDotnet.Artifacts