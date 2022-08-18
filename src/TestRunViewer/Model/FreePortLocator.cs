namespace TestRunViewer.Model;

using System.Net.Sockets;
using System.Net;

internal class FreePortLocator
{
    private static readonly IPEndPoint _defaultLoopbackEndpoint = new(IPAddress.Loopback, port: 0);

    public static int GetAvailablePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(_defaultLoopbackEndpoint);
        return ((IPEndPoint)socket.LocalEndPoint)!.Port;
    }
}