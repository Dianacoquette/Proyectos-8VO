namespace Sensores.Api.Models.Datos.FilterData
{
    public record FilterDataQuery(
        long IpInicioNumero,
        long IpFinNumero,
        string? ClientId,
        DateTime? FechaInicio,

        DateTime? FechaFin,
        int Page,
        int PageSize
    ) : IQuery<FilterDataResult>;

    public record FilterDataResult(
        int Total,
        int Page,
        int PageSize,
        IEnumerable<SensorData> Data
    );

    internal class FilterDataQueryHandler(IDocumentSession documentSession)
        : IQueryHandler<FilterDataQuery, FilterDataResult>
    {
        public async Task<FilterDataResult> Handle(
            FilterDataQuery query,
            CancellationToken cancellationToken)
        {
            var dataQuery = documentSession.Query<SensorData>().AsQueryable();

            if (query.IpInicioNumero > 0 && query.IpFinNumero > 0)
            {
                dataQuery = dataQuery.Where(x =>
                    x.IpDispositivoNumero >= query.IpInicioNumero &&
                    x.IpDispositivoNumero <= query.IpFinNumero);
            }

            if (!string.IsNullOrWhiteSpace(query.ClientId))
            {
                dataQuery = dataQuery.Where(x =>
                    x.ClientId.ToLower().Contains(query.ClientId.ToLower()));
            }

            if (query.FechaInicio.HasValue)
            {
                dataQuery = dataQuery.Where(x =>
                    x.FechaRecepcion >= query.FechaInicio.Value);
            }

            if (query.FechaFin.HasValue)
            {
                dataQuery = dataQuery.Where(x =>
                    x.FechaRecepcion <= query.FechaFin.Value);
            }

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

            var total = await dataQuery.CountAsync(cancellationToken);

            var data = await dataQuery
                .OrderByDescending(x => x.FechaRecepcion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new FilterDataResult(
                total,
                page,
                pageSize,
                data
            );
        }
    }
}