using ConsoleApp21;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            await Server.StartAsync();
        }
        else
        {
            if (args.Length == 1 )
            {
                await Client.StartAsync(args[0]);
            }
        }
    }
}