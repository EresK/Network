using System.Net;
using System.Net.Sockets;

namespace Client;

public class ClientChat
{
    private IPAddress ipAddr;
    private int port;

    public ClientChat(IPAddress ipAddr, int port)
    {
        this.ipAddr = ipAddr;
        this.port = port;
    }

    public void Method()
    {
        using TcpClient client = new TcpClient();
        client.Connect(ipAddr, port);
    }
}