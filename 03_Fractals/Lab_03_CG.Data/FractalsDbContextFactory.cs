using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.Data
{
    public class FractalsDbContextFactory : IDesignTimeDbContextFactory<FractalsDbContext>
    {
        public FractalsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FractalsDbContext>();

            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lab_03_CG");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string dbPath = Path.Combine(folderPath, "fractals.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new FractalsDbContext(optionsBuilder.Options);
        }
    }
}
