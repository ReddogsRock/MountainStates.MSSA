using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using Oqtane.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.TopDogs.Repository
{
    public interface ITopDogsRepository
    {
        Task<IEnumerable<Models.TopDog>> GetTopDogsAsync(int year, int level, string species, int quantity);
    }

    public class TopDogsRepository : ITopDogsRepository, ITransientService
    {
        private readonly IDbContextFactory<TenantDBContext> _factory;

        public TopDogsRepository(IDbContextFactory<TenantDBContext> factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Models.TopDog>> GetTopDogsAsync(int year, int level, string species, int quantity)
        {
            using var db = _factory.CreateDbContext();

            // Add these lines for debugging
            System.Diagnostics.Debug.WriteLine($"Calling SP with: Year={year}, Level={level}, Species={species}, Quantity={quantity}");

            var results = await db.Database.SqlQueryRaw<Models.TopDog>(
                "EXEC dbo.YearEndStandings @Year, @Level, @Species, @Quantity",
                new SqlParameter("@Year", year),
                new SqlParameter("@Level", level),
                new SqlParameter("@Species", species),
                new SqlParameter("@Quantity", quantity)
            ).ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Returned {results?.Count() ?? 0} results");

            return results;
        }
    }
}