using ConsoleApp21;
using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

internal class Client
{
    private static IPEndPoint iPEndPoint;
    private static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
    private static CancellationToken token = cancelTokenSource.Token;

    public static async Task StartAsync(string name)
    {
        iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
        UdpClient ucl = new UdpClient();

        Task receiveTask = ReceiveMessageAsync(ucl); //ожидание сообщения
        Task sendTask = SendCommandAsync(name, ucl); //отправка сообщения 

        await Task.WhenAny(receiveTask, sendTask);
        cancelTokenSource.Cancel(); // Отмена всех задач при завершении

        await Task.WhenAll(receiveTask, sendTask); // Дождаться завершения всех задач
    }

    private static async Task SendCommandAsync(string nik, UdpClient ucl) 
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine("Введите имя получателя:");
                string userName = Console.ReadLine();

                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("Имя получателя не может быть пустым.");
                    continue;
                }

                if (userName.Equals("Server")) //получатель сервер
                {
                    Console.WriteLine("Выберите команду (Register, List, Exit):");
                    string commandInput = Console.ReadLine();

                    if (commandInput.ToLower().Equals("exit"))
                    {
                        cancelTokenSource.Cancel();
                        continue;
                    }

                    Command command;
                    if (!Enum.TryParse(commandInput, true, out command))
                    {
                        Console.WriteLine("Неизвестная команда.");
                        continue;
                    }

                    var serverMessage = new MessageUDP
                    {
                        FromName = nik,
                        ToName = "Server",
                        Command = command
                    };

                    await SendMessageAsync(ucl, serverMessage);
                }
                else //другой получатель
                {
                    Console.WriteLine("Введите сообщение:");
                    string text = Console.ReadLine();

                    var userMessage = new MessageUDP
                    {
                        FromName = nik,
                        ToName = userName,
                        Text = text,
                        Command = Command.Message // Обычное сообщение
                    };

                    await SendMessageAsync(ucl, userMessage);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }

    private static async Task SendMessageAsync(UdpClient ucl, MessageUDP message)
    {
        string json = message.ToJson();
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await ucl.SendAsync(bytes, bytes.Length, iPEndPoint);
    }

    private static async Task ReceiveMessageAsync(UdpClient ucl) //ожидание сообщения
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (ucl.Available > 0)
                {
                    var result = await ucl.ReceiveAsync();
                    string str = Encoding.UTF8.GetString(result.Buffer);
                    var message = MessageUDP.FromJson(str);

                    if (message != null)
                    {
                        // Выводим полученное сообщение
                        Console.WriteLine(message.ToString());

                        if (message.Command == Command.Message && message.Id != null)
                        {
                            // Автоматически подтверждаем получение сообщения
                            await SendConfirmationAsync(ucl, message.FromName, message.Id.Value);
                        }
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static async Task SendConfirmationAsync(UdpClient ucl, string fromName, int messageId)
    {
        var confirmationMessage = new MessageUDP
        {
            FromName = fromName,
            ToName = "Server", // Отправляем серверу
            Command = Command.Confirmation,
            Id = messageId
        };

        string json = confirmationMessage.ToJson();
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await ucl.SendAsync(bytes, bytes.Length, iPEndPoint);
    }
}