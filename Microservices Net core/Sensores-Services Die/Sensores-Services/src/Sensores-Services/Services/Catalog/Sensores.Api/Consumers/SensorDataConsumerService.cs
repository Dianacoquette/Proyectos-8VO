using Confluent.Kafka;
using Sensores.Api.Models;
using System.Text.Json;

namespace Sensores.Api.Consumers;

public class SensorDataConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IDocumentStore _documentStore;
    private readonly string _topic;

    public SensorDataConsumerService(IDocumentStore documentStore, IConfiguration configuration)
    {
        _documentStore = documentStore;
        _topic = configuration["Kafka:Topic"] ?? "datos-sensores";

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "host.docker.internal:9092",
            GroupId = "sensores-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Consumer de Kafka iniciado");

        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var resultado = _consumer.Consume(stoppingToken);

                Console.WriteLine("MENSAJE RECIBIDO");
                Console.WriteLine(resultado.Message.Value);

                var data = JsonSerializer.Deserialize<SensorData>(resultado.Message.Value);

                if (data != null)
                {
                    using var session = _documentStore.LightweightSession();

                    session.Store(data);

                    await session.SaveChangesAsync(stoppingToken);

                    Console.WriteLine("Guardado en PostgreSQL");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR DEL CONSUMER:");
                Console.WriteLine(ex.Message);

                await Task.Delay(1000, stoppingToken);
            }
        }

        _consumer.Close();
    }
}