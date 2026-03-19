using Lab_01_CG.Data.Entities;
using Lab_01_CG.Data.Services.Abstarctions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Lab_01_CG.Data.Services
{
    public class SquareAndCirclesDataService : ISquareAndCirclesDataService
    {
        private readonly IDbContextFactory<SquaresAndCirclesContext> _contextFactory;
        public SquareAndCirclesDataService(IDbContextFactory<SquaresAndCirclesContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        // CREATE
        public async Task AddSquare(Square square)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Squares.Add(square);
            await context.SaveChangesAsync();
        }
        // READ
        public async Task<List<Square>> GetAllSquaresAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var squares = await context.Squares.Include(s => s.InnerCircle).Include(s => s.OuterCircle).AsNoTracking().ToListAsync();
            return squares;
        }
        // UPDATE
        public async Task UpdateSquare(Square square)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Squares.Update(square);
            await context.SaveChangesAsync();
        }
        // DELETE
        public async Task DeleteSquare(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var square = await context.Squares
                    .Include(s => s.InnerCircle)
                    .Include(s => s.OuterCircle)
                    .FirstOrDefaultAsync(s => s.Id == id);
            if (square != null)
            {
                if (square.InnerCircle != null)
                    context.Circles.Remove(square.InnerCircle);
                if (square.OuterCircle != null)
                    context.Circles.Remove(square.OuterCircle);
                context.Squares.Remove(square);

                await context.SaveChangesAsync();
            }
        }
    }
}
