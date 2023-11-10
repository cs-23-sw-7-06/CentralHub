using MQTTnet;
using MQTTnet.Server;
using System.Text;

namespace CentralHub.Api.Services;

internal sealed class MqttServerService
{
    public MqttServer server { get; }

    public MqttServerService()
    {
        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(4000)
            .Build();

        server = new MqttFactory().CreateMqttServer(options);
        server.ClientConnectedAsync += EventListener.OnNewConnection;
        server.ClientDisconnectedAsync += EventListener.OnDisconnect;
        server.ClientAcknowledgedPublishPacketAsync += EventListener.OnNewMessage;
        server.ClientSubscribedTopicAsync += EventListener.OnClientSubscribed;
        server.ClientUnsubscribedTopicAsync += EventListener.OnClientUnsubscribed;
        server.ValidatingConnectionAsync += EventListener.OnValidatingConnection;
        server.ApplicationMessageNotConsumedAsync += EventListener.OnApplicationMessageNotConsumed;

        server.StartAsync().GetAwaiter().GetResult();
    }

    private void Broadcast(string topic, string payload)
    {
        var message = new MqttApplicationMessage()
        {
            Topic = topic,
            PayloadSegment = Encoding.UTF8.GetBytes(payload)
        };

        foreach (var client in server.GetClientsAsync().GetAwaiter().GetResult())
        {
            client.Session.EnqueueApplicationMessageAsync(message);
        }
    }

    private void Unicast(string id, string topic, string payload)
    {
        var client = server.GetClientsAsync().GetAwaiter().GetResult().First(client => client.Id == id);

        var message = new MqttApplicationMessage()
        {
            Topic = topic,
            PayloadSegment = Encoding.UTF8.GetBytes(payload)
        };

        client.Session.EnqueueApplicationMessageAsync(message);
    }

}