namespace Sensores.Api.Models.Dashboard.Charts
{
    public record DashboardChartsQuery() : IQuery<DashboardChartsResult>;

    public record TemperaturaTiempoDto(
        DateTime Fecha,
        double Temperatura,
        string? ClientId,
        string? IpDispositivo
    );

    public record HumedadTiempoDto(
        DateTime Fecha,
        double Humedad,
        string? ClientId,
        string? IpDispositivo
    );

    public record RegistrosPorIpDto(
        string IpDispositivo,
        int Total
    );

    public record PaquetesPorDiaDto(
        DateTime Fecha,
        int Total
    );

    public record PromedioPorIpDto(
        string IpDispositivo,
        double TemperaturaPromedio,
        double HumedadPromedio
    );

    public record DashboardChartsResult(
        IEnumerable<TemperaturaTiempoDto> TemperaturaPorTiempo,
        IEnumerable<HumedadTiempoDto> HumedadPorTiempo,
        IEnumerable<RegistrosPorIpDto> RegistrosPorIp,
        IEnumerable<PaquetesPorDiaDto> PaquetesPorDia,
        IEnumerable<PromedioPorIpDto> PromedioPorIp
    );

    internal class DashboardChartsQueryHandler(IDocumentSession documentSession)
        : IQueryHandler<DashboardChartsQuery, DashboardChartsResult>
    {
        public async Task<DashboardChartsResult> Handle(
            DashboardChartsQuery query,
            CancellationToken cancellationToken)
        {
            var data = await documentSession
                .Query<SensorData>()
                .OrderBy(x => x.FechaRecepcion)
                .ToListAsync(cancellationToken);

            var temperaturaPorTiempo = data.Select(x =>
                new TemperaturaTiempoDto(
                    x.FechaRecepcion,
                    x.Temperatura,
                    x.ClientId,
                    x.IpDispositivo
                ));

            var humedadPorTiempo = data.Select(x =>
                new HumedadTiempoDto(
                    x.FechaRecepcion,
                    x.Humedad,
                    x.ClientId,
                    x.IpDispositivo
                ));

            var registrosPorIp = data
                .Where(x => !string.IsNullOrWhiteSpace(x.IpDispositivo))
                .GroupBy(x => x.IpDispositivo!)
                .Select(g => new RegistrosPorIpDto(
                    g.Key,
                    g.Count()
                ))
                .OrderByDescending(x => x.Total);

            var paquetesPorDia = data
                .GroupBy(x => x.FechaRecepcion.Date)
                .Select(g => new PaquetesPorDiaDto(
                    g.Key,
                    g.Count()
                ))
                .OrderBy(x => x.Fecha);

            var promedioPorIp = data
                .Where(x => !string.IsNullOrWhiteSpace(x.IpDispositivo))
                .GroupBy(x => x.IpDispositivo!)
                .Select(g => new PromedioPorIpDto(
                    g.Key,
                    g.Average(x => x.Temperatura),
                    g.Average(x => x.Humedad)
                ))
                .OrderBy(x => x.IpDispositivo);

            return new DashboardChartsResult(
                temperaturaPorTiempo,
                humedadPorTiempo,
                registrosPorIp,
                paquetesPorDia,
                promedioPorIp
            );
        }
    }
}