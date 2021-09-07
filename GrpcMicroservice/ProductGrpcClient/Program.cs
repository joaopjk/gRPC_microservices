using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Thread.Sleep(5000);
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpHandler = httpHandler });
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            //GetProductAsync
            var response = await client.GetProductAsync(
                new GetProductRequest { ProductId = 1 });
            Console.WriteLine("GetProductAsync:" + Convert.ToString(response));

            //GetAllProductsAsync
            using (var clientData = client.GetAllProducts(new GetAllProductsRequest()))
            {
                while (await clientData.ResponseStream.MoveNext(new CancellationToken()))
                {
                    var current = clientData.ResponseStream.Current;
                    Console.WriteLine(current);
                }
            }

            //GetAllProductsAsync with C# 9
            using var clientData2 = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData2.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }

            //AddProductAsync
            var addProductAsync = await client.AddProductAsync(new AddProductRequest()
            {
                Product = new ProductModel()
                {
                    Name ="Test",
                    Description = "Test",
                    Price = 1000,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });
            Console.WriteLine("AddProductAsync" + addProductAsync.ToString());

            //UpdateProductsAsync
            var updateProductResponse = await client.UpdateProductAsync(
                new UpdateProductRequest()
                {
                    Product = new ProductModel()
                    {
                        ProductId = 1,
                        Name = "Test",
                        Description = "Test",
                        Price = 1000,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                });
            Console.WriteLine("UpdateProductsAsync" + updateProductResponse.ToString());

            //DeleteProductAsync
            var deleteProductAsync = await client.DeleteProductAsync(
                new DeleteProductRequest()
                {
                    ProductId = 2
                });
            Console.WriteLine("DeleteProductAsync" + deleteProductAsync.ToString());

            //InsertBulkAsync
            using var insertBulk = client.InsertBulkProduct();

            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel()
                {
                    Name = $"Product {i}",
                    Description = "Test",
                    Price = 1000,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                await insertBulk.RequestStream.WriteAsync(productModel);
            }

            await insertBulk.RequestStream.CompleteAsync();
            var responseBulk = await insertBulk;
            Console.WriteLine("InsertBulkAsync" + responseBulk.InsertCount);
            Console.ReadKey();
        }
    }
}
