using Sensores.Api.Utils;

namespace Sensores.Api.Models.Datos.FilterData
{
    public record FilterDataResponse(
        int Total,
        int Page,
        int PageSize,
        IEnumerable<SensorData> Data
    );

    public class FilterDataEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/datos/filtrar", async (
                ISender sender,
                string? ipInicio,
                string? ipFin,
                string? clientId,
                DateTime? fechaInicio,
                DateTime? fechaFin,
                int? page,
                int? pageSize) =>
            {
                var query = new FilterDataQuery(
                    IpInicioNumero: IpHelper.ConvertIpToNumber(ipInicio),
                    IpFinNumero: IpHelper.ConvertIpToNumber(ipFin),
                    ClientId: clientId,
                    FechaInicio: fechaInicio,
                    FechaFin: fechaFin,
                    Page: page ?? 1,
                    PageSize: pageSize ?? 20
                );

                var result = await sender.Send(query);

                return Results.Ok(new FilterDataResponse(
                    result.Total,
                    result.Page,
                    result.PageSize,
                    result.Data
                ));
            })
            .WithName("FiltrarDatos")
            .Produces<FilterDataResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Filtrar registros por rango de IP")
            .WithDescription("Consulta registros por rango de IP, ClientId y fechas.");
        }
    }
}