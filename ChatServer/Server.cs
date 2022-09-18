using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer;

public class Server
{
    static TcpListener tcpListener; // сервер для прослушивания
    List<Client> clients = new List<Client>(); // все подключения

    public void AddConnection(Client client)
    {
        clients.Add(client);
    }

    public void RemoveConnection(string id)
    {
        // получаем по id закрытое подключение
        Client client = null;
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Id == id)
                client = clients[i];
            else
                client = clients[0];
        }
        
        // и удаляем его из списка подключений
        if (client != null)
            clients.Remove(client);
    }
    
    // прослушивание входящих подключений
    protected internal void Listen()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 8888);
            tcpListener.Start();
            Console.WriteLine("--------------- Сервер ---------------");
            Console.WriteLine("Сервер запущен. Ожидание подключений...");
 
            while (true)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Client client = new Client(tcpClient, this);
                Thread clientThread = new Thread(client.Process);
                clientThread.Start();
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }
 
    // трансляция сообщения подключенным клиентам
    protected internal void BroadcastMessage(string message, string id)
    {
        byte[] data = Encoding.Unicode.GetBytes(message);
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Id != id) // если id клиента не равно id отправляющего
            {
                clients[i].Stream.Write(data, 0, data.Length); // передача данных
            }
        }
    }
    
    // отключение всех клиентов
    protected internal void Disconnect()
    {
        tcpListener.Stop(); // остановка сервера
 
        for (int i = 0; i < clients.Count; i++)
        {
            clients[i].Close(); // отключение клиентов
        }
        
        Environment.Exit(0); // завершение процесса
    }
}