using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Dogs.Enums;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Finals.Models;
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

            var dog = await db.MSSA_Dogs
                .FirstOrDefaultAsync(d => d.DogId == dogId);

            if (dog != null)
            {
                dog.OwnershipHistory = await db.MSSA_DogOwnershipHistory
                    .Where(h => h.DogId == dogId)
                    .OrderByDescending(h => h.TransferDate)
                    .ToListAsync();
            }

            return dog;
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

        // Narrow, self-service-safe update - touches only IsActive/IsDeceased, not the
        // rest of the dog record (breed, registration, etc.), which stays behind the
        // admin-gated Edit page.
        public async Task<MSSA_Dog> UpdateDogStatusAsync(int dogId, bool isActive, bool isDeceased)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var dog = await db.MSSA_Dogs.FindAsync(dogId);
            if (dog == null)
            {
                return null;
            }

            dog.IsActive = isActive;
            dog.IsDeceased = isDeceased;
            dog.ModifiedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return dog;
        }

        // Records a transfer: logs the outgoing owner to history, then updates the
        // dog's current OwnerName. Self-service, same reasoning as UpdateDogStatusAsync.
        public async Task<MSSA_Dog> TransferDogOwnershipAsync(int dogId, string newOwnerName)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var dog = await db.MSSA_Dogs.FindAsync(dogId);
            if (dog == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            db.MSSA_DogOwnershipHistory.Add(new MSSA_DogOwnershipHistory
            {
                DogId = dogId,
                PreviousOwnerName = dog.OwnerName,
                NewOwnerName = newOwnerName,
                TransferDate = now,
                CreatedDate = now
            });

            dog.OwnerName = newOwnerName;
            dog.ModifiedDate = now;

            await db.SaveChangesAsync();

            dog.OwnershipHistory = await db.MSSA_DogOwnershipHistory
                .Where(h => h.DogId == dogId)
                .OrderByDescending(h => h.TransferDate)
                .ToListAsync();

            return dog;
        }

        // Merges a duplicate dog record into the one being kept: fills in any blank
        // fields on the keeper from the duplicate (never overwrites a value the
        // keeper already has), repoints every table that references the duplicate's
        // DogId, then soft-deactivates the duplicate - matching DeleteDogAsync's
        // existing soft-delete convention rather than a hard row delete. MSSA_Finals
        // (backed by MSSA_FinalsData) has no FK constraint tying it to MSSA_Dogs, but
        // still needs repointing by hand or the merged dog's Finals history would stay
        // split across both ids.
        public async Task<MSSA_Dog> MergeDogsAsync(int keepDogId, int mergeDogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            using var transaction = await db.Database.BeginTransactionAsync();

            var keepDog = await db.MSSA_Dogs.FindAsync(keepDogId);
            var mergeDog = await db.MSSA_Dogs.FindAsync(mergeDogId);

            if (keepDog == null || mergeDog == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(keepDog.Breed)) keepDog.Breed = mergeDog.Breed;
            if (!keepDog.DateOfBirth.HasValue) keepDog.DateOfBirth = mergeDog.DateOfBirth;
            if (string.IsNullOrEmpty(keepDog.RegistrationNumber)) keepDog.RegistrationNumber = mergeDog.RegistrationNumber;
            if (!keepDog.FirstCompetitionYear.HasValue) keepDog.FirstCompetitionYear = mergeDog.FirstCompetitionYear;
            if (string.IsNullOrEmpty(keepDog.OwnerName)) keepDog.OwnerName = mergeDog.OwnerName;
            if (string.IsNullOrEmpty(keepDog.NurseryDocumentFileName))
            {
                keepDog.NurseryDocumentFileName = mergeDog.NurseryDocumentFileName;
                keepDog.NurseryDocumentPath = mergeDog.NurseryDocumentPath;
                keepDog.NurseryDocumentUploadedDate = mergeDog.NurseryDocumentUploadedDate;
            }
            keepDog.ModifiedDate = DateTime.UtcNow;

            var entries = await db.MSSA_Entries.Where(e => e.DogId == mergeDogId).ToListAsync();
            foreach (var e in entries) e.DogId = keepDogId;

            var futurity = await db.MSSA_DogFuturityParticipation.Where(f => f.DogId == mergeDogId).ToListAsync();
            foreach (var f in futurity) f.DogId = keepDogId;

            var ownershipHistory = await db.MSSA_DogOwnershipHistory.Where(h => h.DogId == mergeDogId).ToListAsync();
            foreach (var h in ownershipHistory) h.DogId = keepDogId;

            // ExecuteUpdateAsync (not a normal load-then-save) deliberately, because
            // MSSA_FinalsData's declared key (FinalsResultId) doesn't actually exist as
            // a column on the MSSA_Finals table - loading entities the normal way would
            // make EF select a column that isn't there and throw. A bulk UPDATE avoids
            // ever needing that key.
            await db.MSSA_FinalsData
                .Where(f => f.DogId == mergeDogId)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.DogId, keepDogId));

            mergeDog.IsActive = false;
            mergeDog.Name = $"{mergeDog.Name} (merged into #{keepDogId})";
            mergeDog.ModifiedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            keepDog.OwnershipHistory = await db.MSSA_DogOwnershipHistory
                .Where(h => h.DogId == keepDogId)
                .OrderByDescending(h => h.TransferDate)
                .ToListAsync();

            return keepDog;
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

            participation.Status = FuturityPaymentStatus.PendingPayment;
            participation.CreatedDate = DateTime.UtcNow;
            participation.ModifiedDate = DateTime.UtcNow;

            db.MSSA_DogFuturityParticipation.Add(participation);
            await db.SaveChangesAsync();

            return participation;
        }

        public async Task<MSSA_DogFuturityParticipation> GetFuturityParticipationAsync(int participationId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_DogFuturityParticipation.FindAsync(participationId);
        }

        // Called only from the Stripe webhook once checkout actually completes - never
        // trust a client-side redirect alone to mark something paid. Idempotent: a
        // webhook redelivery just overwrites with the same values.
        public async Task<MSSA_DogFuturityParticipation> MarkFuturityPaymentReceivedAsync(
            int participationId, string stripePaymentIntentId, decimal amount)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var participation = await db.MSSA_DogFuturityParticipation.FindAsync(participationId);
            if (participation == null)
            {
                return null;
            }

            participation.Status = FuturityPaymentStatus.Paid;
            participation.PaymentMethod = "Stripe";
            participation.StripePaymentIntentId = stripePaymentIntentId;
            participation.Amount = amount;
            participation.DateReceived = DateTime.UtcNow.Date;
            participation.ModifiedDate = DateTime.UtcNow;

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

        public async Task<MSSA_DogFuturityParticipation> SaveFuturityDocumentAsync(int participationId, string fileName, string filePath)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var participation = await db.MSSA_DogFuturityParticipation.FindAsync(participationId);
            if (participation == null)
            {
                return null;
            }

            participation.DocumentFileName = fileName;
            participation.DocumentPath = filePath;
            participation.DocumentUploadedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return participation;
        }

        // Entries
        public async Task<IEnumerable<MSSA_DogEntry>> GetDogEntriesAsync(int dogId)
        {
            // TODO: Uncomment when MSSA_Entries, MSSA_Trials, MSSA_Events, MSSA_Handlers, MSSA_Classes tables are created
            
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
          

            // Temporary: Return empty list until other tables are created
            //return await Task.FromResult(new List<MSSA_DogEntry>());
        }
    }
}
