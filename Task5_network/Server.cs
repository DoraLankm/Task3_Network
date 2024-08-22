using ConsoleApp21.Models;
using ConsoleApp21;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ConsoleApp21.Abstraction;

public class Server
{
    private static Dictionary<string, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();
    IMessageSource messageSource;

    public Server (IMessageSource sourse)
    {
        messageSource = sourse;
    }

    void Register(MessageUDP message,IPEndPoint fromep)
    {
        Console.WriteLine("Message Register, name = " + message.FromName);
        clients.Add(message.FromName, fromep);
        using (var ctx = new Context())
        {
            if (ctx.Users.FirstOrDefault(x => x.Name == message.FromName) != null) return;
            ctx.Add(new User {Name = message.FromName });
            ctx.SaveChanges();
        }
    }

    void ConfirmMessageReceived(int? id)
    {
        Console.WriteLine("Message confirmation id=" + id);
        using (var ctx = new Context())
        {
            var msg = ctx.Messages.FirstOrDefault(x => x.Id == id);
            if (msg != null)
            {
               msg.Received = true;
               ctx.SaveChanges();
            }
        }
    }

    void RelyMessage(MessageUDP message)
    {
        int? id = null;
        if (clients.TryGetValue(message.ToName, out IPEndPoint ep))
        {
            using (var ctx = new Context())
            {
                var fromUser = ctx.Users.First(x => x.Name == message.FromName);
                var toUser = ctx.Users.First(x => x.Name == message.ToName);
                var msg = new Message
                {
                    FromUser = fromUser,
                    ToUser = toUser,
                    Received = false,
                    Text = message.Text
                };
                ctx.Messages.Add(msg);
                ctx.SaveChanges();
                id = msg.Id;
            }
            var forwardMessage = new MessageUDP()
            {
                Id = id,
                Command = Command.Message,
                ToName = message.ToName,
                FromName = message.FromName,
                Text = message.Text
            };
            messageSource.SendMessage(forwardMessage, ep);
            Console.WriteLine($"Message Relied, from = {message.FromName} to = {message.ToName}");
        }
        else
        {
            Console.WriteLine("Пользователь не найден.");
        }
    }

    void ProcessMessage(MessageUDP message, IPEndPoint fromep)
    {
        Console.WriteLine($"Получено сообщение от {message.FromName} для {message.ToName} с командой { message.Command}:");
        Console.WriteLine(message.Text);
        if (message.Command == Command.Register)
        {
            Register(message, new IPEndPoint(fromep.Address, fromep.Port));
        }
        if (message.Command == Command.Confirmation)
        {
            Console.WriteLine("Confirmation receiver");
            ConfirmMessageReceived(message.Id);
        }
        if (message.Command == Command.Message)
        {
            RelyMessage(message);
        }

    }

    public void Work()
    {
        Console.WriteLine("UDP Клиент ожидает сообщений...");
        while (true)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var message = messageSource.ReceiveMessage(ref remoteEndPoint);
                ProcessMessage(message, remoteEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
            }
        }
    }






}