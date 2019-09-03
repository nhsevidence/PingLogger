using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NICE.Logging;
using NICE.Logging.Sinks.RabbitMQ;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace PingLogger
{
  public interface ISeriLogger
  {
    void Configure(ILoggerFactory loggerFactory, IConfiguration configuration, IApplicationLifetime appLifetime, ILogEventEnricher eventEnricher);
  }

  public class SeriLogger : ISeriLogger
  {
    public void Configure(ILoggerFactory loggerFactory, IConfiguration configuration, IApplicationLifetime appLifetime, ILogEventEnricher eventEnricher)
    {
      // read appsettings
      var logCfg = configuration.GetSection("Logging");

      loggerFactory.AddConsole(logCfg); // add provider to send logs to System.Console.WriteLine()
      loggerFactory.AddDebug(); // add provider to send logs to System.Diagnostics.Debug.WriteLine()

      int rPort;
      int.TryParse(logCfg["RabbitMQPort"], out rPort);
      var rHost = logCfg["RabbitMQHost"];
      var rVHost = logCfg["RabbitMQVHost"];
      var rUsername = logCfg["RabbitMQUsername"];
      var rPassword = logCfg["RabbitMQPassword"];
      var rExchangeName = logCfg["RabbitMQExchangeName"];
      var rExchangeType = logCfg["RabbitMQExchangeType"];
      var logFilePath = logCfg["SerilogFilePath"];
      var environment = logCfg["Environment"];
      var application = logCfg["Application"];

      LogEventLevel minLevelEnumVal;
      var minLevel = logCfg["SerilogMinLevel"];
      var minLevelCased = char.ToUpper(minLevel[0]) + minLevel.ToLower().Substring(1);
      Enum.TryParse(minLevelCased, out minLevelEnumVal);


      var formatter = new NiceSerilogFormatter(environment, application);

      var rabbit = new RabbitMQConfiguration
      {
        Hostname = rHost,
        VHost = rVHost,
        Port = rPort,
        Username = rUsername,
        Password = rPassword,
        Protocol = RabbitMQ.Client.Protocols.AMQP_0_9_1,
        Exchange = rExchangeName,
        ExchangeType = rExchangeType
      };


      try
      {
        Log.Logger = new LoggerConfiguration()
                          .Enrich.With(eventEnricher)
                          .MinimumLevel.Is(minLevelEnumVal)
                          .WriteTo.RabbitMQ(rabbit, formatter)
                          .WriteTo.RollingFile(formatter, logFilePath, fileSizeLimitBytes: 5000000)
                          .CreateLogger();

        // add serilog provider (this is the hook)
        loggerFactory.AddSerilog();

      }
      catch (Exception ex)
      {
        Log.Logger = GetFileLogger(logFilePath, formatter);
        loggerFactory.AddSerilog();

        var logger = loggerFactory.CreateLogger<SeriLogger>();
        logger.LogError($"Could not connect to RabbitMQ. Hostname:{rHost} Port:{rPort}, Username:{rUsername}, Exchange:{rExchangeName}, ExchangeType:{rExchangeType}, Exception: {ex.Message}");
      }

      // clean up on shutdown
      appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
    }


    private static Serilog.ILogger GetFileLogger(string logFilePath, NiceSerilogFormatter formatter)
    {
      return new LoggerConfiguration()
          .MinimumLevel.Warning()
          .WriteTo.RollingFile(formatter, logFilePath, fileSizeLimitBytes: 5000000)
          .CreateLogger();
    }
  }
}
