using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MSSA.PWA.Services
{
    /// <summary>
    /// Service for offline data storage using IndexedDB
    /// </summary>
    public interface IOfflineStorageService
    {
        // Entry queue management
        Task AddEntryToQueueAsync(MSSA_Entry entry);
        Task<List<MSSA_Entry>> GetUnsyncedEntriesAsync();
        Task<List<MSSA_Entry>> GetEntriesByTrialAsync(int trialId);
        Task MarkEntrySyncedAsync(string tempId);
        Task DeleteEntryAsync(string tempId);
        Task<int> GetQueueCountAsync();

        // Trial caching
        Task CacheTrialAsync(MSSA_Trial trial);
        Task<MSSA_Trial> GetCachedTrialAsync(int trialId);

        // Handler caching
        Task CacheHandlersAsync(List<MSSA_Handler> handlers);
        Task<List<MSSA_Handler>> GetCachedHandlersAsync();

        // Dog caching
        Task CacheDogsAsync(List<MSSA_Dog> dogs);
        Task<List<MSSA_Dog>> GetCachedDogsAsync();

        // Class caching
        Task CacheClassesAsync(List<MSSA_Class> classes);
        Task<List<MSSA_Class>> GetCachedClassesAsync();

        // LocalStorage simple key-value
        Task SetLocalAsync<T>(string key, T value);
        Task<T> GetLocalAsync<T>(string key);
        Task RemoveLocalAsync(string key);
    }
}
