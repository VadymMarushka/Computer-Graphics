using Lab_03_CG.Data;
using Lab_03_CG.Data.Entities;
using Lab_03_CG.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lab_03_CG.Services.Implementaions
{
    // CRUD Service for fractals
    public class FractalDataService : IFractalDataService
    {
        private readonly IDbContextFactory<FractalsDbContext> _contextFactory;

        public FractalDataService(IDbContextFactory<FractalsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // CREATE
        public async Task AddFractalAsync(Fractal fractal, byte[]? imageBytes = null)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                string basePath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                string folderPath = Path.Combine(basePath, "Lab_03_CG");
                string galleryPath = Path.Combine(folderPath, "Gallery");

                if (!Directory.Exists(galleryPath))
                {
                    Directory.CreateDirectory(galleryPath);
                }

                string fileName = $"fractal_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string fullPath = Path.Combine(galleryPath, fileName);

                await File.WriteAllBytesAsync(fullPath, imageBytes);
                fractal.Path = fullPath;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Database.EnsureCreatedAsync();

            if (fractal.Id == Guid.Empty)
            {
                fractal.Id = Guid.NewGuid();
            }

            context.Fractals.Add(fractal);
            await context.SaveChangesAsync();
        }

        // READ
        public async Task<List<Fractal>> GetAllFractalsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var fractals = await context.Fractals.AsNoTracking().ToListAsync();

            return fractals;
        }

        // UPDATE NOT USING THAT
        public async Task UpdateFractalAsync(Fractal fractal)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Fractals.Update(fractal);
            await context.SaveChangesAsync();
        }

        // DELETE
        public async Task DeleteFractalAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var fractal = await context.Fractals.FirstOrDefaultAsync(f => f.Id == id);

            if (fractal != null)
            {
                context.Fractals.Remove(fractal);
                await context.SaveChangesAsync();
            }
        }
    }
}