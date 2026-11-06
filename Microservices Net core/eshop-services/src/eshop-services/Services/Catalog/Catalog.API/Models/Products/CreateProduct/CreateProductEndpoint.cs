using Carter;

namespace Catalog.API.Models.Products.CreateProduct
{
    //validacion de campos
    public record CreateProductRequest(string Name, string Description,
        List<string> Category, string ImageFiles, decimal Price);
    public record CreateProductResponse(Guid Id);

    public class CreateProductEndpoint : ICarterModule
    {
        //genera url
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateProductCommand>();

                var result = await sender.Send(command);
                var response = result.Adapt<CreateProductResponse>();
                //devuelve el id
                return Results.Created($"/products/{response.Id}", response);

            })
                .WithName("CrearProducto")
                .Produces<CreateProductResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Crea un nuevo producto en el catálogo")
                .WithDescription("Crea un nuevo producto y retorna el identificador de la entidad");
        }
    }
}
