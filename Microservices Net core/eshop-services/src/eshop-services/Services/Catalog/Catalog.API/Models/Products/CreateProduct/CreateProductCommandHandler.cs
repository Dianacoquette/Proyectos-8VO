using BuildingBlocks.CQRS;
using Marten;
using System.Windows.Input;
//caso de uso, insertar datos
namespace Catalog.API.Models.Products.CreateProduct
{
    public record CreateProductCommand(string Name, string Description,
        List<string> Category, string ImageFiles, decimal Price)
        : ICommand<CreateProductResult>;
    public record CreateProductResult(Guid Id);
    internal class CreateProcductCommandHandler(IDocumentSession documentSession):
        ICommandHandler<CreateProductCommand,
        CreateProductResult>
    {
        public async Task<CreateProductResult> Handle(CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            //logica para crear el producto
            Product product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                ImageFiles = request.ImageFiles,
                Price = request.Price

            };
            //store logica de base de datos, almacenar datos
            documentSession.Store(product);
            await documentSession.SaveChangesAsync(cancellationToken);
            return new CreateProductResult(product.Id);
        }
    }
    
}
