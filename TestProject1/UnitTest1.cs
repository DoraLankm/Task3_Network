using ConsoleApp21;
using ConsoleApp21.Abstraction;
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
            //Assert.Equals(result.FromName, "Вася");
            Assert.That(Command.Register, Is.EqualTo(result.Command));
        }
    }
}