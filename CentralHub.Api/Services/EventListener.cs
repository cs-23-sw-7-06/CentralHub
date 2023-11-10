using CentralHub.Api.Logging;
using MQTTnet.Server;
using System.Text;


namespace CentralHub.Api.Services;

public static class EventListener
{

    private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
        //Add console output
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

        //Add a custom log provider to write logs to text files
        builder.AddProvider(new FileLoggerProvider(new StreamWriter("log.txt", append: true)));
    });
    private static ILogger logger = loggerFactory.CreateLogger("MqttLogger");

    public static Dictionary<string, List<Dictionary<string, string>>> receivedMessages = new();

    public static Task OnNewConnection(ClientConnectedEventArgs args)
    {
        logger.LogInformation("ESP32 with ID: " + args.ClientId + " Connected!");

        if (!receivedMessages.ContainsKey(args.ClientId))
        {
            receivedMessages.Add(args.ClientId, new List<Dictionary<string, string>>());
        }

        return Task.CompletedTask;
    }

    public static Task OnDisconnect(ClientDisconnectedEventArgs args)
    {
        logger.LogWarning("ESP32 with ID: " + args.ClientId + " Disconnected!");

        return Task.CompletedTask;
    }

    public static Task OnNewMessage(ClientAcknowledgedPublishPacketEventArgs args)
    {
        logger.LogInformation("New Message!");

        return Task.CompletedTask;
    }
    public static Task OnClientSubscribed(ClientSubscribedTopicEventArgs args)
    {
        logger.LogInformation("Client subscribed!");

        return Task.CompletedTask;
    }

    public static Task OnClientUnsubscribed(ClientUnsubscribedTopicEventArgs args)
    {
        logger.LogInformation("Client unsubscribed");

        return Task.CompletedTask;
    }

    public static Task OnValidatingConnection(ValidatingConnectionEventArgs args)
    {
        logger.LogInformation("Validating connection");

        return Task.CompletedTask;
    }

    public static Task OnApplicationMessageNotConsumed(ApplicationMessageNotConsumedEventArgs args)
    {
        logger.LogInformation("Application message received!");
        var payload = Encoding.UTF8.GetString(
            args.ApplicationMessage.PayloadSegment.ToArray(),
            0,
            args.ApplicationMessage.PayloadSegment.Count);

        var topic = args.ApplicationMessage.Topic;
        var id = args.SenderId;

        logger.LogInformation("Node ID:  " + id);
        logger.LogInformation("Topic:    " + topic);
        logger.LogInformation("Payload:  " + payload);

        var messageData = new Dictionary<string, string>
        {
            { "topic", topic },
            { "payload", payload }
        };

        receivedMessages[args.SenderId].Add(messageData);

        return Task.CompletedTask;
    }
}