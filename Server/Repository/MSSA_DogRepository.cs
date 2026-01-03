using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Repository
{
    public class MSSA_DogRepository : IMSSA_DogRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_DogRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<MSSA_Dog>> GetDogsAsync(int moduleId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Dogs
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<MSSA_Dog> GetDogAsync(int dogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Dogs
                .FirstOrDefaultAsync(d => d.DogId == dogId);
        }

        public async Task<MSSA_Dog> AddDogAsync(MSSA_Dog dog)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            dog.CreatedDate = DateTime.UtcNow;
            dog.ModifiedDate = DateTime.UtcNow;

            db.MSSA_Dogs.Add(dog);
            await db.SaveChangesAsync();

            return dog;
        }

        public async Task<MSSA_Dog> UpdateDogAsync(MSSA_Dog dog)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            dog.ModifiedDate = DateTime.UtcNow;

            db.Entry(dog).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return dog;
        }

        public async Task DeleteDogAsync(int dogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var dog = await db.MSSA_Dogs.FindAsync(dogId);
            if (dog != null)
            {
                // Soft delete
                dog.IsActive = false;
                dog.ModifiedDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MSSA_Dog>> SearchDogsAsync(
            string searchTerm = null,
            string breed = null,
            bool? ownerIsMember = null,
            bool? includeInactive = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Dogs.AsQueryable();

            // Apply active filter (default to active only)
            if (!includeInactive.HasValue || !includeInactive.Value)
            {
                query = query.Where(d => d.IsActive);
            }

            // Apply search term (name or owner search)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(d =>
                    d.Name.ToLower().Contains(searchTerm) ||
                    (d.OwnerName != null && d.OwnerName.ToLower().Contains(searchTerm)));
            }

            // Filter by breed
            if (!string.IsNullOrWhiteSpace(breed))
            {
                query = query.Where(d => d.Breed == breed);
            }

            // Filter by owner membership status
            if (ownerIsMember.HasValue)
            {
                query = query.Where(d => d.OwnerIsMSSAMember == ownerIsMember.Value);
            }

            return await query
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        // Futurity
        public async Task<IEnumerable<MSSA_DogFuturityParticipation>> GetDogFuturityParticipationAsync(int dogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_DogFuturityParticipation
                .Where(f => f.DogId == dogId)
                .OrderByDescending(f => f.Year)
                .ToListAsync();
        }

        public async Task<MSSA_DogFuturityParticipation> AddFuturityParticipationAsync(MSSA_DogFuturityParticipation participation)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            db.MSSA_DogFuturityParticipation.Add(participation);
            await db.SaveChangesAsync();

            return participation;
        }

        public async Task DeleteFuturityParticipationAsync(int participationId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var participation = await db.MSSA_DogFuturityParticipation.FindAsync(participationId);
            if (participation != null)
            {
                db.MSSA_DogFuturityParticipation.Remove(participation);
                await db.SaveChangesAsync();
            }
        }

        // Entries
        public async Task<IEnumerable<MSSA_DogEntry>> GetDogEntriesAsync(int dogId)
        {
            // TODO: Uncomment when MSSA_Entries, MSSA_Trials, MSSA_Events, MSSA_Handlers, MSSA_Classes tables are created
            /*
            using var db = await _dbContextFactory.CreateDbContextAsync();
            
            var entries = await (from e in db.MSSA_Entries
                                join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                                join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                where e.DogId == dogId
                                select new MSSA_DogEntry
                                {
                                    EntryId = e.EntryId,
                                    HandlerName = h.FullName,
                                    ClassName = c.ClassName,
                                    SubClassName = c.SubClassName,
                                    Stock = t.Stock,
                                    Placing = e.Placing,
                                    TrialPoints = e.TrialPoints,
                                    TrialDate = t.TrialDate,
                                    EventName = ev.EventName,
                                    Year = ev.PointYear ?? t.TrialDate.Year
                                })
                                .OrderByDescending(e => e.TrialDate)
                                .ToListAsync();

            return entries;
            */

            // Temporary: Return empty list until other tables are created
            return await Task.FromResult(new List<MSSA_DogEntry>());
        }
    }
}
