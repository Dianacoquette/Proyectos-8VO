namespace Sensores.Api.Models.Datos.GetData
{
    public record GetDataQuery() : IQuery<GetDataResult>;

    public record GetDataResult(IEnumerable<SensorData> Data);

    internal class GetDataQueryHandler(
        IDocumentSession documentSession,
        ILogger<GetDataQueryHandler> logger)
        : IQueryHandler<GetDataQuery, GetDataResult>
    {
        public async Task<GetDataResult> Handle(
            GetDataQuery query,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Consultando registros de sensores");

            var data = await documentSession
                .Query<SensorData>()
                .OrderByDescending(x => x.FechaRecepcion)
                .ToListAsync(cancellationToken);

            return new GetDataResult(data);
        }
    }
}