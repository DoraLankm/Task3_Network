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
        //���� ��� �������� �������� ��������� �������� 
        public void TestClientSend()
        {
            MockMessageSource mok = new MockMessageSource(); //���
            Client client = new Client(mok, endPoint, "newClient");
            client.ClientSender("Hello","World");

            MessageUDP get_message = mok.messageList[mok.messageList.Count - 1]; //��������� ������������ ���������
            Assert.IsNotNull(get_message); //��������� �� ������
            Assert.IsNotNull(get_message.ToName); //��� ����������� �������
            Assert.AreEqual("newClient", get_message.FromName); //��� ����������� ���������
            Assert.AreEqual("Hello", get_message.Text); //��� ����������� ���������
            Assert.AreEqual("World", get_message.ToName); //��� ����������� ���������
            Assert.AreEqual(Command.Message, get_message.Command); //������� ���������
        }



        [Test]
        //���� ��� ��������� ���������
        public void ClientListen()
        {
            MockMessageSource mok = new MockMessageSource(); //���
            Client client = new Client(mok, endPoint, "newClient");

            var message = new MessageUDP
            {
                Command = Command.Register,
                FromName = "Client",
                ToName = "Server",
                Text = "",
                Time = DateTime.Now
            };

            mok.messages.Enqueue(message); //��������� ��������� � ����� �������

            client.ClientListener(); //�������

            Assert.AreEqual(0, mok.messages.Count); //�������� �� ������� ���������
            Assert.AreSame(message, mok.messageList[mok.messageList.Count-1]); //������������ ��������� ������ ���������
        }
    }
}