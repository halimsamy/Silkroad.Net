# Silkroad.Net
An unofficial .NET API Wrapper for [Silkroad Online]. This was written to allow easier writing of bots, emulators and cheat guards.

[Silkroad Online]: https://www.silkroadonline.net/

## Motivation
**I made this project for fun!**, I don't care about [Silkroad Online] anymore, and this is the main reason why I'm releasing this to
the public, however, feel free to contribute, discuss things with me if you want, push and clone this repo.
I _may_ push and update this repo from time to time, but I don't really care if things are bad or slow or whatever is it.

## Example
A very simple simple simple Clientless example.
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

private class MyClientlessService {
    [MessageHandler(Opcodes.HANDSHAKE)]
    public Task HandshakeDone(Session session, Message msg) {
        // the handshake has to be completed first. as the client receive 2 HANDSHAKE messages. 
        if (!session.Ready) {
            return Task.CompletedTask;
        }

        var identity = new Message(Opcodes.IDENTITY, true);
        identity.Write("SR_Client");
        identity.Write<byte>(0);
        // await session.SendAsync(identity);
        // we don't want to create a useless StateMachine. so return the send Task and don't await it.
        return session.SendAsync(identity);
    }

    [MessageHandler(Opcodes.IDENTITY)]
    public Task Identity(Session session, Message msg) {
        return Task.CompletedTask;
    }
	
    // TODO: Complete this Clientless connection service,
    // this is just a demo/example.
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
