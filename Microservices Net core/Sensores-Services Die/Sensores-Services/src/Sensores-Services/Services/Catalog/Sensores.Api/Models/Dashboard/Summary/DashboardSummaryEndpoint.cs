namespace Sensores.Api.Models.Dashboard.Summary
{
    public class DashboardSummaryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/dashboard/resumen", async (ISender sender) =>
            {
                var result = await sender.Send(new DashboardSummaryQuery());

                return Results.Ok(result);
            })
            .WithName("DashboardResumen")
            .Produces<DashboardSummaryResult>(StatusCodes.Status200OK)
            .WithSummary("Resumen del dashboard")
            .WithDescription("Devuelve KPIs principales para el dashboard.");
        }
    }
}