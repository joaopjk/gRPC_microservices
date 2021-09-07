using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using ProductStatus = ProductGrpc.Protos.ProductStatus;

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
            if (product == null) throw new RpcException(new Status(StatusCode.NotFound, $"Products whit id={request.ProductId} NotFound!"));
            var productModel = new ProductModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = ProductStatus.Instock,
                CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productsContext.Product.ToListAsync();

            if(productList == null) throw new RpcException(new Status(StatusCode.NotFound, $"Products NotFound!"));

            foreach (var product in productList)
            {
                var productModel = new ProductModel()
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = Product(request.Product);

            _productsContext.Add(product);
            await _productsContext.SaveChangesAsync();

            return ProductToProductModel(product);
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var product = Product(request.Product);
            bool isExist = await _productsContext.Product.AnyAsync(p => p.ProductId == product.ProductId);
            if (!isExist)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Products whit id={product.ProductId} NotFound!"));
            }

            _productsContext.Entry(product).State = EntityState.Modified;
            try
            {
                await _productsContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException )
            {
                throw;
            }

            return ProductToProductModel(product);
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            var product = await _productsContext.Product.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Products whit id={request.ProductId} NotFound!"));
            }

            _productsContext.Product.Remove(product);
            return new DeleteProductResponse()
            {
                Success = await _productsContext.SaveChangesAsync() > 0
            };
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

        private ProductModel ProductToProductModel(Product product)
        {
            return new ProductModel()
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = ProductStatus.Instock,
                CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
            };
        }

        private static Product Product(ProductModel request)
        {
            var product = new Product()
            {
                ProductId = request.ProductId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Status = Models.ProductStatus.Instock,
                CreatedTime = DateTime.UtcNow
            };
            return product;
        }
    }
}
