using System;
using System.Collections.Generic;
using System.Linq;
using ProductGrpc.Models;

namespace ProductGrpc.Data
{
    public class ProductsContextSeed
    {
        public static void SeedAsync(ProductsContext productsContext)
        {
            if (!productsContext.Product.Any())
            {
                var products = new List<Product>()
                {
                    new Product(1, "Xiomi Cell", "Xiomi Cell of last generation", 10000f, ProductStatus.Instock,
                        new DateTime()),
                    new Product(2, "Xiomi Cell X2", "Xiomi Cell of last generation", 12000f, ProductStatus.Instock,
                        new DateTime()),
                    new Product(3, "Xiomi Cell X4", "Xiomi Cell of last generation", 12000f, ProductStatus.Low,
                        new DateTime()),
                    new Product(4, "Xiomi Cell X5", "Xiomi Cell of last generation", 12000f, ProductStatus.None,
                        new DateTime())
                };
                productsContext.Product.AddRange(products);
                productsContext.SaveChanges();
            }
        }
    }
}
