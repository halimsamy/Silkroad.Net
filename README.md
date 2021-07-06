# Silkroad.Net
An unofficial .NET API wrapper for [Silkroad Online]. This was written to allow easier writing of bots, emulators and cheat guards.

[Silkroad Online]: https://www.silkroadonline.net/

## vs SilkroadSecurityAPI
A compression against SilkroadSecurityAPI created by [pushedx].

* Clean code, and powered by .NET Core 3 (requires at least C# 7.0).
* Up to 2~3x faster. (do the benchmarking yourself!)
* Less memory usage. (measure yourself!)
* Using Task-based Asynchronous Pattern (async/await) in favor of Asynchronous Programming Model (APM).
* More abstract with reusable components called services. (check the examples)

## Examples
Connecting to Silkroad's official server. 
```c#
// async Main requires C# 7.0+
private static async Task Main(string[] args) {
    var session = new Session();
    session.RegisterService<MyClientlessService>();
	
    try {
        await session.ConnectAsync("gwgt1.joymax.com", 15779);
        Console.WriteLine("Client connected.");
    } catch (System.Net.Sockets.SocketException) {
        Console.WriteLine("Unable to connect.")
    } 
	
    try {
        await session.RunAsync();
    } catch (Silkroad.Network.RemoteDisconnectedException) {
        Console.WriteLine("You have been disconnected from the server.");
    } catch (Silkroad.Network.Messaging.InvalidMessageException) {
    } catch (Silkroad.Network.Messaging.Handshake.DistortedHandshakeException) {
    } catch (Silkroad.Network.Messaging.Handshake.InvalidHandshakeException) {
    } catch (System.Net.Sockets.SocketException) {
    }
}

// A not-ready-to-use service, missing a lot of implementations,
// However, this is just a demo to show up how to use services.
private class MyClientlessService {
    [MessageHandler(MessageID.HANDSHAKE)]
    public Task HandshakeDone(Session session, Message msg) {
        // The handshake has to be completed first,
        // as the client receives 2 HANDSHAKE messages. 
        if (!session.Ready) {
            return Task.CompletedTask;
        }

        // Prepare the response.
        var identity = new Message(MessageID.IDENTITY, true);
        identity.Write("SR_Client");
        identity.Write<byte>(0);

        // C# tip: return the Send Task, don't create a useless StateMachine.
        // await session.SendAsync(identity);
        return session.SendAsync(identity);
    }

    [MessageHandler(MessageID.IDENTITY)]
    public Task Identity(Session session, Message msg) {
        return Task.CompletedTask;
    }
}
``` 

## License
This project is licensed under the [MIT license].
Also some credits goes to Drew Benton (a.k.a. [pushedx]), [DaxterSoul], Alexander (a.k.a. [Chernobyl]).

[MIT license]: LICENSE
[pushedx]: https://www.elitepvpers.com/forum/members/900141-pushedx.html
[DaxterSoul]: https://github.com/DummkopfOfHachtenduden
[Chernobyl]: https://gitlab.com/Chernobyl_

### Contribution
Unless you explicitly state otherwise, any contribution intentionally submitted
by you, shall be licensed as MIT, without any additional terms or conditions.
