using Lab_03_CG.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.Data
{
    public class FractalsDbContext : DbContext
    {
        public DbSet<Fractal> Fractals { get; set; }

        public FractalsDbContext(DbContextOptions<FractalsDbContext> options) : base(options) { }

        public FractalsDbContext() : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Fractal>()
                .HasDiscriminator<string>("FractalType")
                .HasValue<AlgebraicFractal>("Algebraic")
                .HasValue<GeometricalFractal>("Geometrical");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lab_03_CG");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string dbPath = Path.Combine(folderPath, "fractals.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
    }
}
