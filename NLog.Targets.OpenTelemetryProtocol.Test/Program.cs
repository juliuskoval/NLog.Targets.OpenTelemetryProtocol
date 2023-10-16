var logger = NLog.LogManager.GetCurrentClassLogger();

while (true)
{
    logger.Fatal("hello world");
    
    Thread.Sleep(1000);

    Console.WriteLine($"Sent at {DateTime.Now}.");
}



Console.WriteLine("Hello world");