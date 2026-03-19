using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Lab_01_CG.Data.Entities;
using System.IO;

namespace Lab_01_CG.Data
{
    public class SquaresAndCirclesContext : DbContext
    {
        public DbSet<Square> Squares { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public SquaresAndCirclesContext(DbContextOptions<SquaresAndCirclesContext> options) : base(options) { }
        public SquaresAndCirclesContext() : base() { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Square>()
                .HasOne(s => s.InnerCircle)
                .WithOne().HasForeignKey<Square>(s => s.InnerCircleId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Square>()
                .HasOne(s => s.OuterCircle)
                .WithOne().HasForeignKey<Square>(s => s.OuterCircleId).OnDelete(DeleteBehavior.Cascade);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataBase.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
    }
}
