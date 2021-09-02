using System;

namespace ProductGrpc.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedTime { get; set; }

        public Product(int productId, string name, string description, float price, ProductStatus status, DateTime createdTime)
        {
            ProductId = productId;
            Name = name;
            Description = description;
            Price = price;
            Status = status;
            CreatedTime = createdTime;
        }
    }

    public enum ProductStatus
    {
        Instock = 0,
        Low = 1,
        None = 2
    }
}
