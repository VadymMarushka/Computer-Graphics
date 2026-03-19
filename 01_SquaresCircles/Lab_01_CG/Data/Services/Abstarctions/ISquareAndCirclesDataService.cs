using Lab_01_CG.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_01_CG.Data.Services.Abstarctions
{
    public interface ISquareAndCirclesDataService
    {
        Task AddSquare(Square square);
        Task<List<Square>> GetAllSquaresAsync();
        Task UpdateSquare(Square square);
        Task DeleteSquare(int id);
    }
}
