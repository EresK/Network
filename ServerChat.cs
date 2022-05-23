using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

namespace Server;

public class ServerChat
{
    private IPAddress ipAddr;
    private int port;
    private int maxClient;
    private Encoding encoding;

    private BlockingCollection<TcpClient> clients;
    private BlockingCollection<Task> tasks;

    public ServerChat(IPAddress ipAddr, int port, int maxClient)
    {
        this.ipAddr = ipAddr;
        this.port = port;
        this.maxClient = maxClient;

        encoding = Encoding.UTF8;

        clients = new BlockingCollection<TcpClient>();
        tasks = new BlockingCollection<Task>();
    }

    public void Start()
    {
        try
        {
            TcpListener listener = new TcpListener(ipAddr, port);

            listener.Start();

            // Starting accepting clients
            tasks.Add(Task.Run(() => AcceptClients(listener)));

            while (true)
            {
                string? line = Console.In.ReadLine();
                if (line != null && line.Equals("exit"))
                    break;
            }

            foreach (TcpClient client in clients)
            {
                client.Close();
            }

            Task.WaitAll(tasks.ToArray());

            listener.Stop();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Recieves messages from clients
    /// </summary>
    /// <param name="client"></param>
    private void ListenClient(TcpClient client)
    {
        byte[] buff = new byte[1024];
        int read;

        StringBuilder builder = new StringBuilder();

        try
        {
            while (client.Connected)
            {
                while (client.GetStream().DataAvailable)
                {
                    read = client.GetStream().Read(buff, 0, buff.Length);
                    builder.Append(encoding.GetString(buff, 0, read));
                }

                SendToClients(builder.ToString());

                Thread.Sleep(500);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Sends broadcast like messages
    /// Concurrent safe?
    /// </summary>
    /// <param name="message"></param>
    private void SendToClients(string message)
    {
        foreach (TcpClient client in clients)
        {
            try
            {
                if (client.Connected)
                {
                    client.GetStream().Write(encoding.GetBytes(message));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// Runs in new thread
    /// </summary>
    /// <param name="listener"></param>
    private void AcceptClients(TcpListener listener)
    {
        int clientsCount = 0;
        while (clientsCount < maxClient)
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client);
            tasks.Add(Task.Run(() => ListenClient(client)));
            clientsCount++;
        }
    }

}
