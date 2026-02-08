using Microsoft.JSInterop;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using System.Text.Json;

namespace MSSA.PWA.Services
{
    /// <summary>
    /// Implementation of offline storage using IndexedDB via JavaScript interop
    /// </summary>
    public class OfflineStorageService : IOfflineStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsonSerializerOptions _jsonOptions;

        public OfflineStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Entry queue management
        public async Task AddEntryToQueueAsync(MSSA_Entry entry)
        {
            // Generate a temporary ID for offline tracking
            var tempId = Guid.NewGuid().ToString();

            // Create a dictionary with the entry data plus tempId
            var entryData = new Dictionary<string, object>
            {
                { "tempId", tempId },
                { "trialId", entry.TrialId },
                { "handlerId", entry.HandlerId },
                { "dogId", entry.DogId },
                { "classId", entry.ClassId },
                { "runOrder", entry.RunOrder },
                { "placing", entry.Placing },
                { "runTime", entry.RunTime?.ToString() },
                { "tieBreakerTime", entry.TieBreakerTime?.ToString() },
                { "obstacleScore1", entry.ObstacleScore1 },
                { "obstacleScore2", entry.ObstacleScore2 },
                { "obstacleScore3", entry.ObstacleScore3 },
                { "obstacleScore4", entry.ObstacleScore4 },
                { "obstacleScore5", entry.ObstacleScore5 },
                { "obstacleScore6", entry.ObstacleScore6 },
                { "obstacleScore7", entry.ObstacleScore7 },
                { "obstacleScore8", entry.ObstacleScore8 },
                { "obstacleScore9", entry.ObstacleScore9 },
                { "penalty", entry.Penalty },
                { "trialPoints", entry.TrialPoints },
                { "handlerIsMSSAMember", entry.HandlerIsMSSAMember },
                { "comments", entry.Comments ?? string.Empty }
            };

            await _jsRuntime.InvokeVoidAsync("mssaStorage.addEntry", entryData);
        }

        public async Task<List<MSSA_Entry>> GetUnsyncedEntriesAsync()
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getUnsyncedEntries");
                return DeserializeEntries(result);
            }
            catch
            {
                return new List<MSSA_Entry>();
            }
        }

        public async Task<List<MSSA_Entry>> GetEntriesByTrialAsync(int trialId)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getEntriesByTrial", trialId);
                return DeserializeEntries(result);
            }
            catch
            {
                return new List<MSSA_Entry>();
            }
        }

        public async Task MarkEntrySyncedAsync(string tempId)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.markEntrySynced", tempId);
        }

        public async Task DeleteEntryAsync(string tempId)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.deleteEntry", tempId);
        }

        public async Task<int> GetQueueCountAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<int>("mssaStorage.getQueueCount");
            }
            catch
            {
                return 0;
            }
        }

        // Trial caching
        public async Task CacheTrialAsync(MSSA_Trial trial)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.cacheTrial", trial);
        }

        public async Task<MSSA_Trial> GetCachedTrialAsync(int trialId)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getCachedTrial", trialId);
                if (result.ValueKind == JsonValueKind.Null || result.ValueKind == JsonValueKind.Undefined)
                    return null;

                return JsonSerializer.Deserialize<MSSA_Trial>(result.GetRawText(), _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        // Handler caching
        public async Task CacheHandlersAsync(List<MSSA_Handler> handlers)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.cacheHandlers", handlers);
        }

        public async Task<List<MSSA_Handler>> GetCachedHandlersAsync()
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getCachedHandlers");
                if (result.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<MSSA_Handler>>(result.GetRawText(), _jsonOptions)
                           ?? new List<MSSA_Handler>();
                }
                return new List<MSSA_Handler>();
            }
            catch
            {
                return new List<MSSA_Handler>();
            }
        }

        // Dog caching
        public async Task CacheDogsAsync(List<MSSA_Dog> dogs)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.cacheDogs", dogs);
        }

        public async Task<List<MSSA_Dog>> GetCachedDogsAsync()
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getCachedDogs");
                if (result.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<MSSA_Dog>>(result.GetRawText(), _jsonOptions)
                           ?? new List<MSSA_Dog>();
                }
                return new List<MSSA_Dog>();
            }
            catch
            {
                return new List<MSSA_Dog>();
            }
        }

        // Class caching
        public async Task CacheClassesAsync(List<MSSA_Class> classes)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.cacheClasses", classes);
        }

        public async Task<List<MSSA_Class>> GetCachedClassesAsync()
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getCachedClasses");
                if (result.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<MSSA_Class>>(result.GetRawText(), _jsonOptions)
                           ?? new List<MSSA_Class>();
                }
                return new List<MSSA_Class>();
            }
            catch
            {
                return new List<MSSA_Class>();
            }
        }

        // LocalStorage simple key-value
        public async Task SetLocalAsync<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.setLocal", key, value);
        }

        public async Task<T> GetLocalAsync<T>(string key)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("mssaStorage.getLocal", key);
                if (result.ValueKind == JsonValueKind.Null || result.ValueKind == JsonValueKind.Undefined)
                    return default;

                return JsonSerializer.Deserialize<T>(result.GetRawText(), _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public async Task RemoveLocalAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("mssaStorage.removeLocal", key);
        }

        // Helper method to deserialize entries from JsonElement
        private List<MSSA_Entry> DeserializeEntries(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
                return new List<MSSA_Entry>();

            var entries = new List<MSSA_Entry>();

            foreach (var item in jsonElement.EnumerateArray())
            {
                try
                {
                    var entry = new MSSA_Entry
                    {
                        TrialId = GetIntValue(item, "trialId"),
                        HandlerId = GetIntValue(item, "handlerId"),
                        DogId = GetIntValue(item, "dogId"),
                        ClassId = GetIntValue(item, "classId"),
                        RunOrder = GetNullableIntValue(item, "runOrder"),
                        Placing = GetNullableIntValue(item, "placing"),
                        RunTime = GetTimeSpanValue(item, "runTime"),
                        TieBreakerTime = GetTimeSpanValue(item, "tieBreakerTime"),
                        ObstacleScore1 = GetNullableDecimalValue(item, "obstacleScore1"),
                        ObstacleScore2 = GetNullableDecimalValue(item, "obstacleScore2"),
                        ObstacleScore3 = GetNullableDecimalValue(item, "obstacleScore3"),
                        ObstacleScore4 = GetNullableDecimalValue(item, "obstacleScore4"),
                        ObstacleScore5 = GetNullableDecimalValue(item, "obstacleScore5"),
                        ObstacleScore6 = GetNullableDecimalValue(item, "obstacleScore6"),
                        ObstacleScore7 = GetNullableDecimalValue(item, "obstacleScore7"),
                        ObstacleScore8 = GetNullableDecimalValue(item, "obstacleScore8"),
                        ObstacleScore9 = GetNullableDecimalValue(item, "obstacleScore9"),
                        Penalty = GetNullableDecimalValue(item, "penalty"),
                        TrialPoints = GetNullableIntValue(item, "trialPoints"),
                        HandlerIsMSSAMember = GetBoolValue(item, "handlerIsMSSAMember"),
                        Comments = GetStringValue(item, "comments")
                    };

                    entries.Add(entry);
                }
                catch
                {
                    // Skip invalid entries
                    continue;
                }
            }

            return entries;
        }

        // Helper methods for safe JSON parsing
        private int GetIntValue(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
                ? prop.GetInt32()
                : 0;
        }

        private int? GetNullableIntValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                    return prop.GetInt32();
                if (prop.ValueKind == JsonValueKind.Null)
                    return null;
            }
            return null;
        }

        private decimal? GetNullableDecimalValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                    return prop.GetDecimal();
                if (prop.ValueKind == JsonValueKind.Null)
                    return null;
            }
            return null;
        }

        private bool GetBoolValue(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.True;
        }

        private string GetStringValue(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString()
                : string.Empty;
        }

        private TimeSpan? GetTimeSpanValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                var value = prop.GetString();
                if (TimeSpan.TryParse(value, out var timeSpan))
                    return timeSpan;
            }
            return null;
        }
    }
}
