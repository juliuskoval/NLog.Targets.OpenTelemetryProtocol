var logger = NLog.LogManager.GetCurrentClassLogger();

while (true)
{
    // logger.Debug("Hello world");
    // logger.Info("Hello world");
    // logger.Warn("Hello world");
    // logger.Error("Hello world");
    logger.Fatal("Hello world");
    
    Thread.Sleep(100000);

    Console.WriteLine($"Sent at {DateTime.Now}.");
}