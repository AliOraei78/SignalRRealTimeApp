# SignalRRealTimeApp

A sample SignalR project demonstrating real-time development skills in .NET

## Project Progress

### Day 1

* Created an ASP.NET Core Web App project
* Installed and configured SignalR
* Implemented a simple `ChatHub` with message broadcasting capability
* Built a test page using a JavaScript client
* 
### Day 2

* Expanded Hub basics
* Implemented various broadcasting methods:

  * `Clients.All`
  * `Clients.Caller`
  * `Clients.Others`
* Enhanced the test UI with multiple messaging methods

### Day 3

* Implemented `OnConnectedAsync` and `OnDisconnectedAsync`
* Managed connection lifecycle
* Displayed online users using `ConcurrentDictionary`
* Broadcasted user join and leave notifications

### Day 4

* Implemented Groups in SignalR
* Added `JoinGroup`, `LeaveGroup`, and `SendToGroup` methods
* Enabled sending messages to specific groups
* Set default membership to the `General` group

### Day 5

* Added ASP.NET Core Identity
* Implemented `SendToUser`
* Enabled private messaging to specific users
* Applied authorization to the Hub

### Day 6

* Implemented Strongly Typed Hubs using the `IChatClient` interface
* Improved type safety in server–client communication
* Refactored Hub methods
