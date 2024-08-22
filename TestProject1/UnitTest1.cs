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

        //[Test]
        //public void TestReceiveMessage()
        //{
        //    _source = new MockMessageSource();
        //    var result = _source.ReceiveMessage(ref endPoint);
        //    Assert.IsNotNull(result);
        //    Assert.IsNull(result.Text);
        //    Assert.IsNotNull(result.FromName);
        //    Assert.That(Command.Register, Is.EqualTo(result.Command));
        //}

        [Test]
        //тест для проверки отправки сообщений клиентом 
        public void TestClientSend()
        {
            MockMessageSource mok = new MockMessageSource(); //мок
            Client client = new Client(mok, endPoint, "newClient");
            client.ClientSender("Hello","World");

            MessageUDP get_message = mok.messageList[mok.messageList.Count - 1]; //последнее отправленное сообщение
            Assert.IsNotNull(get_message); //сообщение не пустое
            Assert.IsNotNull(get_message.ToName); //имя отправителя указано
            Assert.AreEqual("newClient", get_message.FromName); //имя отправителя совпадает
            Assert.AreEqual("Hello", get_message.Text); //имя отправителя совпадает
            Assert.AreEqual("World", get_message.ToName); //имя отправителя совпадает
            Assert.AreEqual(Command.Message, get_message.Command); //команда совпадает
        }



        [Test]
        //тест для прослушки сообщения
        public void ClientListen()
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

            mok.messages.Enqueue(message); //добавляем сообщение в конец очереди

            client.ClientListener(); //слушаем

            Assert.AreEqual(0, mok.messages.Count); //удаление из очереди сообщения
            Assert.AreSame(message, mok.messageList[mok.messageList.Count-1]); //отправленные сообщения должны совпадать
        }
    }
}