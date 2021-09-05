using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Protos;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductsContext _productsContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductsContext productsContext, ILogger<ProductService> logger)
        {
            _productsContext = productsContext ?? throw new ArgumentNullException(nameof(productsContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productsContext.Product.FindAsync(request.ProductId);
            _logger.Log(LogLevel.Information, $"GetProduct returns: {Convert.ToString(request)}");
            var productModel = new ProductModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = ProductStatus.Instock,
                CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
            };
            return productModel;
        }

        public override Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            return base.GetAllProducts(request, responseStream, context);
        }

        public override Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            return base.AddProduct(request, context);
        }

        public override Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            return base.UpdateProduct(request, context);
        }

        public override Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            return base.DeleteProduct(request, context);
        }

        public override Task<InsertBulkProductResponse> InsertBulkProduct(IAsyncStreamReader<ProductModel> requestStream, ServerCallContext context)
        {
            return base.InsertBulkProduct(requestStream, context);
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, "Test class [ProductService]");
            return base.Test(request, context);
        }
    }
}
