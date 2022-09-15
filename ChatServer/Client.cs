using System.Net.Sockets;
using System.Text;

namespace ChatServer;

public class Client
{
    protected internal string Id { get; }
    protected internal NetworkStream Stream { get; private set; }
    
    private string Username;
    private TcpClient TcpClient;
    private Server Server;

    public Client(TcpClient tcpTcpClient, Server server)
    {
        this.Id = Guid.NewGuid().ToString();
        this.TcpClient = tcpTcpClient;
        this.Server = server;

        server.AddConnection(this);
    }
    
    // чтение входящего сообщения и преобразование в строку
    private string GetMessage()
    {
        byte[] data = new byte[64]; // буфер для получаемых данных
        StringBuilder builder = new StringBuilder();
        int bytes;
        do
        {
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (Stream.DataAvailable);
 
        return builder.ToString();
    }

    // Обработка сообщения
    public void Process()
    {
        try
        {
            Stream = TcpClient.GetStream();
            // получаем имя пользователя
            Username = GetMessage();
 
            string message = String.Format($"{Username} вошел в чат.");
            // посылаем сообщение о входе в чат всем подключенным пользователям
            Server.BroadcastMessage(message, this.Id);
            Console.WriteLine(message);
            // в бесконечном цикле получаем сообщения от клиента
            while (true)
            {
                try
                {
                    message = String.Format($"{Username}: {GetMessage()}");
                    Console.WriteLine(message);
                    Server.BroadcastMessage(message, this.Id);
                }
                catch
                {
                    message = String.Format($"{Username} покинул чат.");
                    Console.WriteLine(message);
                    Server.BroadcastMessage(message, this.Id);
                    break;
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            // в случае выхода из цикла закрываем ресурсы
            Server.RemoveConnection(this.Id);
            Close();
        }
    }
    
    // закрытие подключения
    public void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (TcpClient != null)
            TcpClient.Close();
    }
}