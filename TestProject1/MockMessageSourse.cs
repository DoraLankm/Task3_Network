using ConsoleApp21;
using ConsoleApp21.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    internal class MockMessageSource : IMessageSource
    {
        
        private Queue<MessageUDP> messages = new(); // Очередь сообщений для имитации приёма сообщений
        private IMessageSource server; // Ссылка на сервер для управления его работой
        private IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0); // Конечная точка для имитации

 // Конструктор класса, который инициализирует начальные сообщения в очереди
        public MockMessageSource()
        {
            messages.Enqueue(new MessageUDP { Command = Command.Register, FromName = "Вася" });
            messages.Enqueue(new MessageUDP { Command = Command.Register, FromName = "Юля" });
            messages.Enqueue(new MessageUDP
            {
                Command = Command.Message,
                FromName = "Юля",
                ToName = "Вася",
                Text = "От Юли"
            });
            messages.Enqueue(new MessageUDP
            {
                Command = Command.Message,
                FromName = "Вася",
                ToName = "Юля",
                Text = "От Васи"
            });
        }

        public MessageUDP ReceiveMessage(ref IPEndPoint iPEndPoint)
        {
            return messages.Peek();
        }

        public void SendMessage(MessageUDP message, IPEndPoint iPEndPoint)
        {
            messages.Enqueue(message);
        }


    }
}

