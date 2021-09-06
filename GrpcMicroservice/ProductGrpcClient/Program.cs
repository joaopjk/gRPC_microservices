using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
            Console.WriteLine(Convert.ToString(response));
        }
    }
}
