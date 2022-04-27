using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace DeliveryWebApplication
{
    public class Deletable
    {
        public bool Deleted { get; set; } = false;
    }

    public static class DeletableExtensions
    {
        public static IQueryable<T> Alive<T>(this IQueryable<T> seq) where T : Deletable => seq.Where(x => !x.Deleted);
        public static IEnumerable<T> Alive<T>(this IEnumerable<T> seq) where T : Deletable => seq.Where(x => !x.Deleted);

        public static async ValueTask<T?> AliveFindAsync<T>(this DbSet<T> seq, int? id) where T : Deletable
        {
            var res = await seq.FindAsync(id);
            if (res is null || res.Deleted)
                return null;
            else
                return res;
        }

    }

    public static class Utils
    {
        public static string FormattedWeight(int? weight)
        {
            if (weight is null)
                return "—";
            if (weight < 1000)
                return weight + " г";
            else
                return ((decimal)weight / 1000).ToString("0.###") + " кг";
        }

        public static string FormattedWeight(decimal? weight) => FormattedWeight((int?)(weight * 1000));

        public static void SetCulture()
        {
            var cul = new System.Globalization.CultureInfo("en-US");
            cul.NumberFormat.NumberDecimalSeparator = ".";
            cul.NumberFormat.NumberGroupSeparator = "";
            Thread.CurrentThread.CurrentCulture = cul;

        }
    }

    public partial class DeliveryContext : DbContext
    {
        public DeliveryContext()
        {
        }

        public DeliveryContext(DbContextOptions<DeliveryContext> options)
            : base(options)
        {
            //if (Products.Count() != 0)
            //    return;
            //dynamic arr;
            //using (StreamReader r = new StreamReader(@"products-in-fora\products-processed.json"))
            //{
            //    string json = r.ReadToEnd();
            //    arr = JsonConvert.DeserializeObject(json);
            //}
            //foreach (var product in arr)
            //{
            //    string title = product.title;
            //    decimal? weight = product.weight == "" ? null : (decimal?)product.weight;
            //    decimal price = (decimal)product.price;
            //    string country = product.country;
            //    string trademark = product.trademark;
            //    string category = product.category;

            //    int countryId = Countries.FirstOrDefault(c => c.Name == country)?.Id ?? 0;
            //    if (countryId == 0)
            //    {
            //        var countryObj = new Country { Name = country };
            //        Add(countryObj);
            //        SaveChanges();
            //        countryId = countryObj.Id;
            //    }

            //    int trademarkId = Trademarks.FirstOrDefault(t => t.Name == trademark)?.Id ?? 0;
            //    if (trademarkId == 0)
            //    {
            //        var trademarkObj = new Trademark { Name = trademark };
            //        Add(trademarkObj);
            //        SaveChanges();
            //        trademarkId = trademarkObj.Id;
            //    }

            //    int categoryId = Categories.FirstOrDefault(c => c.Name == category)?.Id ?? 0;
            //    if (categoryId == 0)
            //    {
            //        var categoryObj = new Category { Name = category };
            //        Add(categoryObj);
            //        SaveChanges();
            //        categoryId = categoryObj.Id;
            //    }

            //    var productObj = new Product { Name = title, CountryId = countryId, TrademarkId = trademarkId, CategoryId = categoryId, Weight = weight };
            //    Add(productObj);
            //    SaveChanges();
            //    int productId = productObj.Id;

            //    ProductsInShops.Add(new ProductInShop { ProductId = productId, ShopId = 1, Price = price });
            //    SaveChanges();
            //}
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Country> Countries { get; set; } = null!;
        public virtual DbSet<Courier> Couriers { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderItem> OrderItems { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductInShop> ProductsInShops { get; set; } = null!;
        public virtual DbSet<Shop> Shops { get; set; } = null!;
        public virtual DbSet<Trademark> Trademarks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server= DESKTOP-0IQFBK0\\SQLEXPRESS; Database=Delivery; Trusted_Connection=True; ");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Courier>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.DeliveryPrice).HasColumnType("money");

                entity.HasOne(d => d.Courier)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CourierId)
                    .HasConstraintName("FK_Orders_Couriers");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Orders_Customers");

                entity.HasOne(d => d.Shop)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ShopId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Orders_Shops");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItems_Orders");

                entity.HasOne(d => d.ProductInShop)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductInShopId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItems_OrderItems");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_Categories");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_Countries");

                entity.HasOne(d => d.Trademark)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.TrademarkId)
                    .HasConstraintName("FK_Products_Trademarks");

                entity.Property(d => d.Weight).HasPrecision(8, 3);
            });

            modelBuilder.Entity<ProductInShop>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("money");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductsInShops)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductsInShops_Products");

                entity.HasOne(d => d.Shop)
                    .WithMany(p => p.ProductsInShops)
                    .HasForeignKey(d => d.ShopId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductsInShops_Shops");
            });

            modelBuilder.Entity<Shop>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.Site).HasMaxLength(50);
            });

            modelBuilder.Entity<Trademark>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
