using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShoppingApp.Data;

public partial class ShoppingApplicationContext : DbContext
{
    public ShoppingApplicationContext()
    {
    }

    public ShoppingApplicationContext(DbContextOptions<ShoppingApplicationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BilledProduct> BilledProducts { get; set; }

    public virtual DbSet<BillingTable> BillingTables { get; set; }

    public virtual DbSet<ItemsTable> ItemsTables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-98T4PT03\\sqlexpress;Initial Catalog=Shopping_Application;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BilledProduct>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Billed_Products");

            entity.Property(e => e.BillId).HasColumnName("BillID");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("productName");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
        });

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserName).HasName("Pk_UserName");

            entity.HasIndex(e => e.BusinessName, "BusinessName_Unique").IsUnique();

            entity.HasIndex(e => e.UserName, "UserName_Unique").IsUnique();

            entity.Property(e => e.UserName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.BusinessName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
