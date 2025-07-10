using Microsoft.EntityFrameworkCore;
using RedisCaching.Models;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace RedisCaching.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Generate dummy data (600 products)
            var products = GenerateDummyProducts(600);

            // Seed data into Products table
            modelBuilder.Entity<Product>().HasData(products);
        }

        private static Product[] GenerateDummyProducts(int count)
        {
            var products = new Product[count];

            // Static categories to use for seeding
            var categories = new[] { "Fruits", "Vegetables", "Beverages" };

            // Static price and quantity ranges for better control over the seed data
            var prices = new[] { 100, 200, 500, 1000 }; // Example fixed prices
            var quantities = new[] { 50, 100, 150 }; // Example fixed quantities

            for (int i = 1; i <= count; i++)
            {
                // Generate a fixed product name
                var name = $"Product {i}";

                // Randomly assign a category (but keep it consistent)
                var category = categories[i % categories.Length]; // Use modulo to cycle through categories

                // Use a static price and quantity from predefined values
                var price = prices[i % prices.Length]; // Cycle through the predefined prices
                var quantity = quantities[i % quantities.Length]; // Cycle through the predefined quantities

                products[i - 1] = new Product
                {
                    Id = i, // Ensure unique Id for seed data
                    Name = name,
                    Category = category,
                    Price = price,
                    Quantity = quantity
                };
            }

            return products;
        }

    }
}
