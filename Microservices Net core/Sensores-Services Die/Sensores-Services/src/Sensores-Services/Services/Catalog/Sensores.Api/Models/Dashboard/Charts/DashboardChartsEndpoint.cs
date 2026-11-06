namespace Sensores.Api.Models.Dashboard.Charts
{
    public class DashboardChartsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/dashboard/graficas", async (ISender sender) =>
            {
                var result = await sender.Send(new DashboardChartsQuery());

                return Results.Ok(result);
            })
            .WithName("DashboardGraficas")
            .Produces<DashboardChartsResult>(StatusCodes.Status200OK)
            .WithSummary("Datos para gráficas")
            .WithDescription("Devuelve datos agrupados para las gráficas del dashboard.");
        }
    }
}