using ConsoleApp21;
using ConsoleApp21.Abstraction;
using NUnit.Framework.Interfaces;
using System.Net;

namespace TestProject1
{
    public class Tests
    {
        IMessageSource _source;
        IPEndPoint endPoint;
        [SetUp]
        public void Setup()
        {
            endPoint = new IPEndPoint(IPAddress.Any,0);
        }

        [Test]
        public void TestReceiveMessage()
        {
            _source = new MockMessageSource();
            var result = _source.ReceiveMessage(ref endPoint);
            Assert.IsNotNull(result);
            Assert.IsNull(result.Text);
            Assert.IsNotNull(result.FromName);
            Assert.That(Command.Register, Is.EqualTo(result.Command));
        }

        [Test]
        //тест для проверки отправки сообщений клиентом 
        public void TestClientSend()
        {
            MockMessageSource mok = new MockMessageSource(); //мок
            Client client = new Client(mok, endPoint, "newClient");
            client.ClientSender("Hello","World");

            Assert.IsTrue(mok.messageList.Count > 0);
            MessageUDP get_message = mok.messageList.Last(); // Берем последнее сообщение из списка
            Assert.IsNotNull(get_message); //сообщение не пустое
            Assert.IsNotNull(get_message.ToName); //имя отправителя указано
            Assert.AreEqual("newClient", get_message.FromName); //имя отправителя совпадает
            Assert.AreEqual("Hello", get_message.Text); //имя отправителя совпадает
            Assert.AreEqual("World", get_message.ToName); //имя отправителя совпадает
            Assert.AreEqual(Command.Message, get_message.Command); //команда совпадает
        }



        [Test]
        //тест для прослушки сообщения
        public void ClientListenOnce()
        {
            MockMessageSource mok = new MockMessageSource(); //мок
            Client client = new Client(mok, endPoint, "newClient");

            var message = new MessageUDP
            {
                Command = Command.Register,
                FromName = "Client",
                ToName = "Server",
                Text = "",
                Time = DateTime.Now
            };

            int count = mok.messages.Count; //число сообщений до помещения в очередь

            mok.messages.Enqueue(message); //добавляем сообщение в конец очереди

            client.ClientListenerOnce(); //слушаем

            Assert.AreEqual(count, mok.messages.Count); //удаление из очереди сообщения
        }
    }
}