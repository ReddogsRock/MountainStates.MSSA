using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.TrialSecretary.Repository
{
    public class TrialSecretaryRepository : ITrialSecretaryRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public TrialSecretaryRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            // Get events from the last 30 days
            var cutoffDate = DateTime.Today.AddDays(-30);

            var events = await db.MSSA_Events
                .Where(e => e.IsActive && e.StartDate >= cutoffDate)
                .OrderBy(e => e.StartDate)
                .Select(e => new RecentEventDto
                {
                    EventId = e.EventId,
                    EventIdentifier = e.EventIdentifier,
                    EventName = e.EventName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    City = e.City,
                    StateCode = e.StateCode
                })
                .ToListAsync();

            // Get trials for these events
            var eventIds = events.Select(e => e.EventId).ToList();
            
            var trials = await db.MSSA_Trials
                .Where(t => eventIds.Contains(t.EventId))
                .OrderBy(t => t.TrialDate)
                .ToListAsync();

            // Get all active classes
            var allClasses = await db.MSSA_Classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.PrintOrder ?? int.MaxValue)
                .ToListAsync();

            // Populate trials for each event with filtered classes
            foreach (var evt in events)
            {
                // Get the full event details to check flags
                var fullEvent = await db.MSSA_Events
                    .FirstOrDefaultAsync(e => e.EventId == evt.EventId);

                // Filter classes based on event settings
                var filteredClasses = allClasses
                    .Where(c => IsClassAllowedForEvent(c, fullEvent))
                    .Select(c => new ClassOptionDto
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName,
                        SubClassName = c.SubClassName
                    })
                    .ToList();

                evt.Trials = trials
                    .Where(t => t.EventId == evt.EventId)
                    .Select(t => new TrialSummaryDto
                    {
                        TrialId = t.TrialId,
                        TrialIdentifier = t.TrialIdentifier,
                        TrialName = t.TrialName,
                        TrialDate = t.TrialDate,
                        Stock = t.Stock,
                        AvailableClasses = filteredClasses
                    })
                    .ToList();
            }

            return events;
        }

        /// <summary>
        /// Determines if a class is allowed for an event based on event settings
        /// </summary>
        private bool IsClassAllowedForEvent(MSSA_Class classItem, MSSA_Event evt)
        {
            if (evt == null) return true; // If no event, allow all classes

            var fullClassName = $"{classItem.ClassName} {classItem.SubClassName}".ToLower();

            // Check Horseback/OnFoot
            bool hasHorseback = fullClassName.Contains("horseback") || fullClassName.Contains("horse back");
            bool hasOnFoot = fullClassName.Contains("on foot") || fullClassName.Contains("on-foot") || fullClassName.Contains("onfoot");

            // If class is horseback but event doesn't allow horseback, exclude it
            if (hasHorseback && !evt.Horseback)
                return false;

            // If class is on-foot but event doesn't allow on-foot, exclude it
            if (hasOnFoot && !evt.OnFoot)
                return false;

            // Check class levels (Open, Nursery, Intermediate, Novice, Junior)
            if (fullClassName.Contains("open") && !evt.Open)
                return false;

            if (fullClassName.Contains("nursery") && !evt.Nursery)
                return false;

            if (fullClassName.Contains("intermediate") && !evt.Intermediate)
                return false;

            if (fullClassName.Contains("novice") && !evt.Novice)
                return false;

            if (fullClassName.Contains("junior") && !evt.Junior)
                return false;

            // Check arena/field
            bool hasArena = fullClassName.Contains("arena");
            bool hasField = fullClassName.Contains("field");

            if (hasArena && !evt.Arena)
                return false;

            if (hasField && !evt.Field)
                return false;

            // If we get here, the class is allowed
            return true;
        }

        public async Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Handlers.Where(h => h.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(h =>
                    h.FirstName.ToLower().Contains(searchTerm) ||
                    h.LastName.ToLower().Contains(searchTerm) ||
                    h.FullName.ToLower().Contains(searchTerm));
            }

            var handlers = await query
                .OrderBy(h => h.LastName)
                .ThenBy(h => h.FirstName)
                .Take(50)
                .ToListAsync();

            // Get active memberships
            var handlerIds = handlers.Select(h => h.HandlerId).ToList();
            var currentYear = DateTime.Today.Year;
            var activeMemberships = await db.MSSA_HandlerMemberships
                .Where(m => handlerIds.Contains(m.HandlerId) &&
                           m.StartYear <= currentYear &&
                           m.EndYear >= currentYear &&
                           m.IsActive &&
                           m.DateReceived.HasValue)
                .Select(m => m.HandlerId)
                .ToListAsync();

            return handlers.Select(h => new HandlerSearchDto
            {
                HandlerId = h.HandlerId,
                FullName = h.FullName,
                City = h.City,
                StateCode = h.StateCode,
                Email = h.Email,
                Phone = h.Phone,
                HasActiveMembership = activeMemberships.Contains(h.HandlerId)
            }).ToList();
        }

        public async Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var handler = await db.MSSA_Handlers
                .Where(h => h.HandlerId == handlerId && h.IsActive)
                .FirstOrDefaultAsync();

            if (handler == null)
                return null;

            var currentYear = DateTime.Today.Year;
            var hasActiveMembership = await db.MSSA_HandlerMemberships
                .AnyAsync(m => m.HandlerId == handlerId &&
                              m.StartYear <= currentYear &&
                              m.EndYear >= currentYear &&
                              m.IsActive &&
                              m.DateReceived.HasValue);

            return new HandlerSearchDto
            {
                HandlerId = handler.HandlerId,
                FullName = handler.FullName,
                City = handler.City,
                StateCode = handler.StateCode,
                Email = handler.Email,
                Phone = handler.Phone,
                HasActiveMembership = hasActiveMembership
            };
        }

        public async Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto handlerDto)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var handler = new MSSA_Handler
            {
                FirstName = handlerDto.FirstName,
                LastName = handlerDto.LastName,
                Email = handlerDto.Email,
                Phone = handlerDto.Phone,
                City = handlerDto.City,
                StateCode = handlerDto.StateCode,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsActive = true
            };

            db.MSSA_Handlers.Add(handler);
            await db.SaveChangesAsync();

            return handler;
        }

        public async Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Dogs.Where(d => d.IsActive && !d.IsDeceased);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(searchTerm));
            }

            return await query
                .OrderBy(d => d.Name)
                .Take(50)
                .Select(d => new DogSearchDto
                {
                    DogId = d.DogId,
                    Name = d.Name,
                    Breed = d.Breed,
                    OwnerName = d.OwnerName,
                    Age = d.DateOfBirth.HasValue
                        ? DateTime.Today.Year - d.DateOfBirth.Value.Year
                        : (int?)null
                })
                .ToListAsync();
        }

        public async Task<DogSearchDto> GetDogByIdAsync(int dogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var dog = await db.MSSA_Dogs
                .Where(d => d.DogId == dogId && d.IsActive && !d.IsDeceased)
                .FirstOrDefaultAsync();

            if (dog == null)
                return null;

            return new DogSearchDto
            {
                DogId = dog.DogId,
                Name = dog.Name,
                Breed = dog.Breed,
                OwnerName = dog.OwnerName,
                Age = dog.DateOfBirth.HasValue
                    ? DateTime.Today.Year - dog.DateOfBirth.Value.Year
                    : (int?)null
            };
        }

        public async Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var dog = new MSSA_Dog
            {
                Name = dogDto.Name,
                Breed = dogDto.Breed,
                OwnerName = dogDto.OwnerName,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeceased = false,
                IsSold = false
            };

            db.MSSA_Dogs.Add(dog);
            await db.SaveChangesAsync();

            return dog;
        }

        public async Task<List<int>> CreateEntriesAsync(CreateEntriesDto entriesDto, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entryIds = new List<int>();

            // Check if handler has active membership
            var currentYear = DateTime.Today.Year;
            var handlerIsMember = await db.MSSA_HandlerMemberships
                .AnyAsync(m => m.HandlerId == entriesDto.HandlerId &&
                              m.StartYear <= currentYear &&
                              m.EndYear >= currentYear &&
                              m.IsActive &&
                              m.DateReceived.HasValue);

            foreach (var trialEntry in entriesDto.TrialEntries)
            {
                var entry = new MSSA_Entry
                {
                    TrialId = trialEntry.TrialId,
                    HandlerId = entriesDto.HandlerId,
                    DogId = entriesDto.DogId,
                    ClassId = trialEntry.ClassId,
                    HandlerIsMSSAMember = handlerIsMember,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    EnteredBy = userId,
                    ModifiedBy = userId
                };

                db.MSSA_Entries.Add(entry);
                await db.SaveChangesAsync();
                
                entryIds.Add(entry.EntryId);
            }

            return entryIds;
        }

        public async Task<EventEntriesSummaryDto> GetEventEntriesAsync(int eventId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            // Get event details
            var evt = await db.MSSA_Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (evt == null)
                return null;

            var result = new EventEntriesSummaryDto
            {
                EventId = eventId,
                EventName = evt.EventName
            };

            // Get all trials for this event
            var trials = await db.MSSA_Trials
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.TrialDate)
                .ToListAsync();

            foreach (var trial in trials)
            {
                var trialDto = new TrialEntriesDto
                {
                    TrialId = trial.TrialId,
                    TrialName = trial.TrialName,
                    TrialIdentifier = trial.TrialIdentifier,
                    Stock = trial.Stock
                };

                // Get all entries for this trial
                var entries = await db.MSSA_Entries
                    .Where(e => e.TrialId == trial.TrialId)
                    .OrderBy(e => e.RunOrder ?? int.MaxValue)
                    .ToListAsync();

                if (entries.Any())
                {
                    // Get handler names
                    var handlerIds = entries.Select(e => e.HandlerId).Distinct().ToList();
                    var handlers = await db.MSSA_Handlers
                        .Where(h => handlerIds.Contains(h.HandlerId))
                        .ToDictionaryAsync(h => h.HandlerId, h => h.FullName);

                    // Get dog names
                    var dogIds = entries.Select(e => e.DogId).Distinct().ToList();
                    var dogs = await db.MSSA_Dogs
                        .Where(d => dogIds.Contains(d.DogId))
                        .ToDictionaryAsync(d => d.DogId, d => d.Name);

                    // Get class info
                    var classIds = entries.Select(e => e.ClassId).Distinct().ToList();
                    var classes = await db.MSSA_Classes
                        .Where(c => classIds.Contains(c.ClassId))
                        .ToDictionaryAsync(c => c.ClassId);

                    // Group by class
                    var entriesByClass = entries.GroupBy(e => e.ClassId);

                    foreach (var classGroup in entriesByClass)
                    {
                        var classInfo = classes.ContainsKey(classGroup.Key) 
                            ? classes[classGroup.Key] 
                            : null;

                        var classDto = new ClassEntriesDto
                        {
                            ClassId = classGroup.Key,
                            ClassName = classInfo?.ClassName ?? "Unknown",
                            SubClassName = classInfo?.SubClassName
                        };

                        foreach (var entry in classGroup)
                        {
                            classDto.Entries.Add(new EntryDetailDto
                            {
                                EntryId = entry.EntryId,
                                HandlerName = handlers.ContainsKey(entry.HandlerId) 
                                    ? handlers[entry.HandlerId] 
                                    : "Unknown",
                                DogName = dogs.ContainsKey(entry.DogId) 
                                    ? dogs[entry.DogId] 
                                    : "Unknown",
                                HandlerIsMSSAMember = entry.HandlerIsMSSAMember,
                                RunOrder = entry.RunOrder
                            });
                        }

                        trialDto.Classes.Add(classDto);
                    }
                }

                result.Trials.Add(trialDto);
            }

            return result;
        }
    }
}
