using ConsoleApp21.Models;
using ConsoleApp21;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;

internal class Server
{
    private static Dictionary<string, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();
    private static UdpClient udpClient;


    private static async Task RegisterAsync(MessageUDP message, IPEndPoint fromIPEndPoint) //регистрация пользователя 
    {
        try
        {
            Console.WriteLine("Запрос регистрации от " + message.FromName);
            if (clients.TryAdd(message.FromName, fromIPEndPoint))
            {
                using (var ctx = new Context())
                {
                    if (ctx.Users.Any(x => x.Name == message.FromName))
                    {
                        return; //если пользоатель существует, возврат
                    }

                    ctx.Users.Add(new User { Name = message.FromName }); //добавление пользователя в базу 
                    await ctx.SaveChangesAsync(); //сохранить изменения

                }
                await SendMessageToSenderAsync(message.FromName, fromIPEndPoint, "Регистрация успешна");
            }


        }
        catch(Exception ex)
        {
            Console.WriteLine($"Ошибка!{ex.Message }");
        }
        
    }

    private static async Task ConfirmAsync(int? messageId) //подтверждение получения сообщения
    {

        using (var ctx = new Context())
        {
            var msg = await ctx.Messages.FirstOrDefaultAsync(m => m.Id == messageId);

            if (msg != null)
            {
                msg.Received = true; //статус прочтения 
                await ctx.SaveChangesAsync();
                Console.WriteLine($"Сообщение {messageId} получено.");
            }
        }
    }

    private static async Task SendMessageAsync(MessageUDP message, IPEndPoint fromep) //отправка сообщения 
    {
        int? id = null;

        if (clients.TryGetValue(message.ToName, out IPEndPoint ep)) //получатель найден в базе
        {
            using (var ctx = new Context())
            {
                var fromUser = await ctx.Users.FirstAsync(x => x.Name == message.FromName);
                var toUser = await ctx.Users.FirstAsync(x => x.Name == message.ToName);

                var msg = new Message
                {
                    FromUser = fromUser,
                    ToUser = toUser,
                    Received = false,
                    Text = message.Text
                };

                ctx.Messages.Add(msg);
                await ctx.SaveChangesAsync();
                id = msg.Id;
            }

            var forwardMessageJson = new MessageUDP
            {
                Id = id,
                Command = Command.Message,
                ToName = message.ToName,
                FromName = message.FromName,
                Text = message.Text
            }.ToJson();

            byte[] forwardBytes = Encoding.ASCII.GetBytes(forwardMessageJson);
            await udpClient.SendAsync(forwardBytes, forwardBytes.Length, ep);
            await SendMessageToSenderAsync(message.FromName, fromep, $"Сообщение отправлено пользователю {message.ToName}.");
        }
        else
        {
            await SendMessageToSenderAsync(message.FromName, fromep, $"Пользователь не найден {message.ToName}.");
        }
    }

    private static async Task ProcessMessageAsync(MessageUDP message, IPEndPoint fromep) //обработка полученного сообщения
    {
        try
        {
            Console.WriteLine($"Получено сообщение от {message.FromName} для {message.ToName} с командой {message.Command}: {message.Text}");

            switch (message.Command)
            {
                case Command.Register: //регистрация
                    await RegisterAsync(message, fromep);
                    break;
                case Command.Confirmation: //подтверждение получения сообщения
                    await ConfirmAsync(message.Id);
                    break;
                case Command.Message: //отправка сообщения пользователю
                    await SendMessageAsync(message, fromep);
                    break;
                case Command.List: //получить непрочитанные сообщения 
                    await ListAsync(message.FromName);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static async Task StartAsync() //запуск сервера
    {
        try
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            udpClient = new UdpClient(12345);

            Console.WriteLine("Сервер ожидает сообщения от клиента");

            while (true)
            {
                if (udpClient.Available > 0) //вхоядщее сообщение есть 
                {
                    var result = await udpClient.ReceiveAsync(); 
                    string receivedData = Encoding.ASCII.GetString(result.Buffer); //преобразуем в строку 
                    MessageUDP message = MessageUDP.FromJson(receivedData); //строку преобразуем в сообщение 
                    await ProcessMessageAsync(message, result.RemoteEndPoint); //обработка полученного сообщения 

                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task ListAsync(string fromName)
    {
        try
        {
            if (clients.TryGetValue(fromName, out IPEndPoint clientEndPoint)) //пользователь найден в базе
            {
                using (var ctx = new Context())
                {
                    // Получаем все непрочитанные сообщения для пользователя
                    var unreadMessages = await ctx.Messages
                        .Where(m => m.ToUser.Name == fromName && !m.Received)
                        .ToListAsync();

                    foreach (var message in unreadMessages)
                    {
                        // MessageUDP для каждого непрочитанного сообщения
                        var messageUDP = new MessageUDP
                        {
                            Id = message.Id, 
                            Command = Command.Message,
                            FromName = message.FromUser.Name,
                            ToName = message.ToUser.Name,
                            Text = message.Text,
                            Time = DateTime.Now
                        };

                        // Отправляем сообщение клиенту
                        string messageJson = messageUDP.ToJson();
                        byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
                        await udpClient.SendAsync(messageBytes, messageBytes.Length, clientEndPoint);

                        // После отправки отмечаем сообщение как прочитанное
                        message.Received = true;
                    }

                    // Сохраняем изменения в базе данных
                    await ctx.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обработке команды List: " + ex.Message);
        }
    }

    private static async Task SendMessageToSenderAsync(string toName, IPEndPoint clientEndPoint, string messageText)
    {
        var newtMessage = new MessageUDP
        {
            FromName = "Server",
            ToName = toName,
            Text = messageText,
            Command = Command.Confirmation 
        };

        string json = newtMessage.ToJson();
        byte[] messageBytes = Encoding.UTF8.GetBytes(json);
        await udpClient.SendAsync(messageBytes, messageBytes.Length, clientEndPoint);
    }
}