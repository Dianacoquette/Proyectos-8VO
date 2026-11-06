using Carter;
using Sensores.Api.Services;
using Sensores.Api.Models;

namespace Sensores.Api.Models.Datos
{
    public class SendSensorEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/datos", async (
                SensorData request,
                IKafkaProducerService producer) =>
            {
                Console.WriteLine("🔥 ENDPOINT HIT");

                var mensaje = System.Text.Json.JsonSerializer.Serialize(request);

                await producer.EnviarDatoAsync(mensaje);

                return Results.Ok("Enviado a Kafka");
            });
        }
    }
}