namespace Sensores.Api.Models.Dashboard.Summary
{
    public record DashboardSummaryQuery() : IQuery<DashboardSummaryResult>;

    public record DashboardSummaryResult(
        int TotalPaquetesRecibidos,
        int TotalRegistrosGuardados,
        double TemperaturaActual,
        double HumedadActual,
        string? UltimaIpDispositivo,
        string? UltimoClientId,
        DateTime? UltimoPaqueteRecibido,
        int TotalIpsDetectadas,
        int TotalClientesDetectados
    );

    internal class DashboardSummaryQueryHandler(IDocumentSession documentSession)
        : IQueryHandler<DashboardSummaryQuery, DashboardSummaryResult>
    {
        public async Task<DashboardSummaryResult> Handle(
            DashboardSummaryQuery query,
            CancellationToken cancellationToken)
        {
            var data = await documentSession
                .Query<SensorData>()
                .ToListAsync(cancellationToken);

            var ultimo = data
                .OrderByDescending(x => x.FechaRecepcion)
                .FirstOrDefault();

            return new DashboardSummaryResult(
                TotalPaquetesRecibidos: data.Count,
                TotalRegistrosGuardados: data.Count,
                TemperaturaActual: ultimo?.Temperatura ?? 0,
                HumedadActual: ultimo?.Humedad ?? 0,
                UltimaIpDispositivo: ultimo?.IpDispositivo,
                UltimoClientId: ultimo?.ClientId,
                UltimoPaqueteRecibido: ultimo?.FechaRecepcion,
                TotalIpsDetectadas: data
                    .Where(x => !string.IsNullOrWhiteSpace(x.IpDispositivo))
                    .Select(x => x.IpDispositivo)
                    .Distinct()
                    .Count(),
                TotalClientesDetectados: data
                    .Where(x => !string.IsNullOrWhiteSpace(x.ClientId))
                    .Select(x => x.ClientId)
                    .Distinct()
                    .Count()
            );
        }
    }
}