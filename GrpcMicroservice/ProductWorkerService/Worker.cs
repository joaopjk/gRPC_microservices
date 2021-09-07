using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProductGrpc.Protos;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.Sleep(5000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var httpHandler = new HttpClientHandler();
                httpHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using var channel = GrpcChannel.ForAddress(_configuration["WorkerService:ServerUrl"], new GrpcChannelOptions { HttpHandler = httpHandler });
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                var addProductAsync = await client.AddProductAsync(new AddProductRequest()
                {
                    Product = new ProductModel()
                    {
                        Name = _configuration["WorkerService:ProductName"] + DateTime.Now,
                        Description = _configuration["WorkerService:ProductName"],
                        Price = 1000,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                });
                Console.WriteLine("AddProductAsync" + addProductAsync.ToString());

                await Task.Delay(Convert.ToInt32(_configuration["WorkerService:TaskInterval"]), stoppingToken);
            }
        }
    }
}
