using System.Net.Sockets;
using System.Text;
 
namespace ChatClient;

internal class Program
{
    static string username;
    private const string host = "127.0.0.1";
    private const int port = 8888;
    static TcpClient client;
    static NetworkStream stream;

    // отправка сообщений
    static void SendMessage()
    {
        Console.WriteLine("Введите сообщение: ");
         
        while (true)
        {
            string message = Console.ReadLine();
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
    
    // получение сообщений
    static void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                byte[] data = new byte[64]; // буфер для получаемых данных
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                string message = builder.ToString();
                Console.WriteLine(message);
            }
            catch
            {
                Console.WriteLine("Подключение прервано!"); //соединение было прервано
                Console.ReadLine();
                Disconnect();
            }
        }
    }

    static void Disconnect()
    {
        if (stream != null)
            stream.Close(); //отключение потока
        if (client != null)
            client.Close(); //отключение клиента
        
        Environment.Exit(0); //завершение процесса
    }
    
    static void Main(string[] args)
    {
        Console.Write("Введите свое имя: ");
        username = Console.ReadLine();
        client = new TcpClient();
        try
        {
            client.Connect(host, port); //подключение клиента
            stream = client.GetStream(); // получаем поток

            if (username != null)
            {
                byte[] data = Encoding.Unicode.GetBytes(username);
                stream.Write(data, 0, data.Length);
            }

            // запускаем новый поток для получения данных
            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start(); //старт потока
            Console.WriteLine($"Добро пожаловать, {username}.");
            SendMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }
}