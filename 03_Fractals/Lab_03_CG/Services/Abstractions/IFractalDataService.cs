using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab_03_CG.Data.Entities;

namespace Lab_03_CG.Services.Abstractions
{
    public interface IFractalDataService
    {
        Task AddFractalAsync(Fractal fractal, byte[]? imageBytes = null);
        Task<List<Fractal>> GetAllFractalsAsync();
        Task UpdateFractalAsync(Fractal fractal);
        Task DeleteFractalAsync(Guid id);
    }
}