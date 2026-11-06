namespace Sensores.Api.Models.Datos.GetData
{
    public record GetDataResponse(IEnumerable<SensorData> Data);

    public class GetDataEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/datos", async (ISender sender) =>
            {
                var result = await sender.Send(new GetDataQuery());

                return Results.Ok(new GetDataResponse(result.Data));
            })
            .WithName("GetDatos")
            .Produces<GetDataResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Consultar registros")
            .WithDescription("Retorna todos los registros guardados por los sensores.");
        }
    }
}