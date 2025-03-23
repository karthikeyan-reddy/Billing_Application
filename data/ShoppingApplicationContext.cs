using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShoppingApp.data;

public partial class ShoppingApplicationContext : DbContext
{
    public ShoppingApplicationContext()
    {
    }

    public ShoppingApplicationContext(DbContextOptions<ShoppingApplicationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BillingTable> BillingTables { get; set; }

    public virtual DbSet<ItemsTable> ItemsTables { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("Server=LAPTOP-98T4PT03\\sqlexpress;Database=Shopping_Application;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingTable>(entity =>
        {
            entity.HasKey(e => e.BillNumber);

            entity.ToTable("Billing_Table");

            entity.Property(e => e.BillNumber).ValueGeneratedNever();
            entity.Property(e => e.AmountPaid).HasColumnType("money");
            entity.Property(e => e.BillDateTime).HasColumnType("datetime");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(300)
                .HasColumnName("Customer_Address");
            entity.Property(e => e.CustomerEmailId)
                .HasMaxLength(50)
                .HasColumnName("customer_EmailId");
            entity.Property(e => e.CustomerMobileNumber).HasColumnName("Customer_MobileNumber");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasColumnName("customerName");
            entity.Property(e => e.ItemsPurchased)
                .HasMaxLength(500)
                .HasColumnName("items_purchased");
            entity.Property(e => e.MessageStatus).HasColumnName("message_status");
            entity.Property(e => e.PaymentMode).HasColumnName("payment_mode");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("money")
                .HasColumnName("Total_Amount");
        });

        modelBuilder.Entity<ItemsTable>(entity =>
        {
            entity.HasKey(e => e.ItemNumber);

            entity.ToTable("ITEMS_TABLE");

            entity.Property(e => e.ItemNumber).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.ItemImage)
                .HasMaxLength(250)
                .HasColumnName("Item_Image");
            entity.Property(e => e.ItemName).HasMaxLength(200);
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.StockAvailable).HasColumnName("stock_Available");
            entity.Property(e => e.TotalStock).HasColumnName("Total_Stock");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
