using Cart.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Cart.Api.Data
{
    public class CartContext : DbContext
    {
        public CartContext([NotNull] DbContextOptions options) : base(options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
                throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<CartEntity>().HasMany<ItemEntity>().WithOne().HasForeignKey(i => i.CartId);

            modelBuilder.Entity<ItemEntity>().HasKey(i => new { i.CartId, i.ProductId });
        }

        public DbSet<CartEntity> Carts { get; set; }

        public DbSet<ItemEntity> Items { get; set; }
    }
}
