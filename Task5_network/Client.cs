using ConsoleApp21;
using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using ConsoleApp21.Abstraction;

internal class Client
{
    private readonly IMessageSource _messageSource;
    private readonly IPEndPoint IPEndPoint;
    private readonly string _name;

    public Client(IMessageSource messageSource, IPEndPoint iPEndPoint, string name)
    {
        _messageSource = messageSource;
        IPEndPoint = iPEndPoint;
        _name = name;
    }

    private void Register()
    {
        var message = new MessageUDP()
        {
            Command = Command.Register,
            FromName = _name,
            ToName = "Server",
            Time = DateTime.Now
        };
        _messageSource.SendMessage(message, IPEndPoint);

    }

    public void ClientSender()
    {

        while (true)
        {
            Console.WriteLine("Введите сообщение");
            string message = Console.ReadLine();
            Console.WriteLine("Введите имя");
            string name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }
            MessageUDP messageUDP = new MessageUDP
            {
                Command = Command.Message,
                ToName = name,
                Text = message,
                FromName = _name,
                Time = DateTime.Now
            };
            _messageSource.SendMessage(messageUDP, IPEndPoint);
        }
    }

    public void ClientListener()
    {
        Register();
        IPEndPoint ep = new IPEndPoint(IPEndPoint.Address, IPEndPoint.Port);
        while (true)
        {
            MessageUDP messageUDP = _messageSource.ReceiveMessage(ref ep);
            Console.WriteLine(messageUDP);

        }
    }
}