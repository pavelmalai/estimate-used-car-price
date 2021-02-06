using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UsedCarsPrice.Common.Models
{
    public partial class UsedCarsDbContext : DbContext
    {
        public UsedCarsDbContext()
        {
        }

        public UsedCarsDbContext(DbContextOptions<UsedCarsDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ScrapingLogs> Scrapinglogs { get; set; }
        public virtual DbSet<UsedCarModel> Usedcars { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("Server=localhost;User Id=pavel;Password=parola;Database=UsedCars");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScrapingLogs>(entity =>
            {
                entity.ToTable("scrapinglogs");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.ArticlesScrapingDuration)
                    .IsRequired()
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.NewArticlesScraped)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.NewUrlsAdded)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TotalDuration)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.TotalValidRecords)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UrlScrapingDuration)
                    .IsRequired()
                    .HasColumnType("varchar(200)");
            });

            modelBuilder.Entity<UsedCarModel>(entity =>
            {
                entity.ToTable("usedcars");

                entity.HasIndex(e => e.Url)
                    .HasName("URL")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Year).HasColumnType("text");

                entity.Property(e => e.Body).HasColumnType("text");

                entity.Property(e => e.Fuel).HasColumnType("text");

                entity.Property(e => e.Color).HasColumnType("text");

                entity.Property(e => e.Gearbox).HasColumnType("text");

                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.GoupId).HasColumnType("char(36)");

                entity.Property(e => e.LastModified).HasColumnType("datetime");

                entity.Property(e => e.Brand).HasColumnType("text");

                entity.Property(e => e.Model).HasColumnType("text");

                entity.Property(e => e.OferitDe).HasColumnType("text");

                entity.Property(e => e.Scraped)
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Stare).HasColumnType("text");

                entity.Property(e => e.Title).HasColumnType("text");

                entity.Property(e => e.Url)
                    .HasColumnName("URL")
                    .HasColumnType("varchar(500)");
            });
        }
    }
}
