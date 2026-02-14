using System.Net.Http.Json;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MSSA.PWA.Services
{
    /// <summary>
    /// Service for making API calls to the Oqtane backend
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly int _moduleId;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            // Get the module ID from configuration
            // You'll need to set this in appsettings.json
            _moduleId = configuration.GetValue<int>("ModuleId", 1);
        }

        // Entry operations
        public async Task<ApiResponse<MSSA_Entry>> SaveEntryAsync(MSSA_Entry entry)
        {
            try
            {
                // Set audit fields
                entry.CreatedDate = DateTime.Now;
                entry.ModifiedDate = DateTime.Now;

                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/MSSA_Entry?moduleid={_moduleId}", 
                    entry);
                
                if (response.IsSuccessStatusCode)
                {
                    var savedEntry = await response.Content.ReadFromJsonAsync<MSSA_Entry>();
                    return ApiResponse<MSSA_Entry>.SuccessResponse(savedEntry);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return ApiResponse<MSSA_Entry>.ErrorResponse(
                        $"Failed to save entry: {errorMessage}", 
                        (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<MSSA_Entry>.ErrorResponse($"Error saving entry: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MSSA_Entry>>> GetEntriesByTrialAsync(int trialId)
        {
            try
            {
                // Note: The endpoint returns EntryListItem, but we'll use MSSA_Entry for now
                var entries = await _httpClient.GetFromJsonAsync<List<MSSA_Entry>>(
                    $"/api/MSSA_Entry/trial/{trialId}?moduleid={_moduleId}");
                
                return ApiResponse<List<MSSA_Entry>>.SuccessResponse(entries ?? new List<MSSA_Entry>());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MSSA_Entry>>.ErrorResponse($"Error getting entries: {ex.Message}");
            }
        }

        // Trial operations
        public async Task<ApiResponse<MSSA_Trial>> GetTrialAsync(int trialId)
        {
            try
            {
                var trial = await _httpClient.GetFromJsonAsync<MSSA_Trial>(
                    $"/api/MSSA_Trial/{trialId}?moduleid={_moduleId}");
                
                return ApiResponse<MSSA_Trial>.SuccessResponse(trial);
            }
            catch (Exception ex)
            {
                return ApiResponse<MSSA_Trial>.ErrorResponse($"Error getting trial: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MSSA_Trial>>> GetActiveTrialsAsync()
        {
            try
            {
                // Get all events (which contain trials)
                var events = await _httpClient.GetFromJsonAsync<List<MSSA_Event>>(
                    $"/api/MSSA_Event?moduleid={_moduleId}");
                
                if (events == null || !events.Any())
                    return ApiResponse<List<MSSA_Trial>>.SuccessResponse(new List<MSSA_Trial>());

                // Get trials for recent events (last 90 days and upcoming)
                var recentEvents = events.Where(e => 
                    e.StartDate >= DateTime.Now.AddDays(-90) ||
                    e.EndDate >= DateTime.Now).ToList();

                var allTrials = new List<MSSA_Trial>();

                // Get trials for each recent event
                foreach (var evt in recentEvents)
                {
                    try
                    {
                        var trials = await _httpClient.GetFromJsonAsync<List<MSSA_Trial>>(
                            $"/api/MSSA_Trial/event/{evt.EventId}?moduleid={_moduleId}");
                        
                        if (trials != null)
                            allTrials.AddRange(trials);
                    }
                    catch
                    {
                        // Skip events that fail
                        continue;
                    }
                }

                // Sort by date
                allTrials = allTrials.OrderByDescending(t => t.TrialDate).ToList();
                
                return ApiResponse<List<MSSA_Trial>>.SuccessResponse(allTrials);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MSSA_Trial>>.ErrorResponse($"Error getting trials: {ex.Message}");
            }
        }

        // Handler operations
        public async Task<ApiResponse<List<MSSA_Handler>>> GetActiveHandlersAsync()
        {
            try
            {
                Console.WriteLine($"Calling: /api/MSSA_Handler?moduleid={_moduleId}");
                var handlers = await _httpClient.GetFromJsonAsync<List<MSSA_Handler>>(
                    $"/api/MSSA_Handler?moduleid={_moduleId}");

                // Filter to active only
                var activeHandlers = handlers?.Where(h => h.IsActive).ToList() ?? new List<MSSA_Handler>();

                return ApiResponse<List<MSSA_Handler>>.SuccessResponse(activeHandlers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR getting handlers: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return ApiResponse<List<MSSA_Handler>>.ErrorResponse($"Error getting handlers: {ex.Message}");
            }
        }

        public async Task<ApiResponse<MSSA_Handler>> GetHandlerAsync(int handlerId)
        {
            try
            {
                var handler = await _httpClient.GetFromJsonAsync<MSSA_Handler>(
                    $"/api/MSSA_Handler/{handlerId}?moduleid={_moduleId}");
                
                return ApiResponse<MSSA_Handler>.SuccessResponse(handler);
            }
            catch (Exception ex)
            {
                return ApiResponse<MSSA_Handler>.ErrorResponse($"Error getting handler: {ex.Message}");
            }
        }

        // Dog operations
        public async Task<ApiResponse<List<MSSA_Dog>>> GetActiveDogsAsync()
        {
            try
            {
                var dogs = await _httpClient.GetFromJsonAsync<List<MSSA_Dog>>(
                    $"/api/MSSA_Dog?moduleid={_moduleId}");
                
                // Filter to active only
                var activeDogs = dogs?.Where(d => d.IsActive && !d.IsDeceased && !d.IsSold).ToList() 
                                ?? new List<MSSA_Dog>();
                
                return ApiResponse<List<MSSA_Dog>>.SuccessResponse(activeDogs);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MSSA_Dog>>.ErrorResponse($"Error getting dogs: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MSSA_Dog>>> GetDogsByHandlerAsync(int handlerId)
        {
            try
            {
                // Get all dogs and filter client-side
                // (We could add a server endpoint for this later for better performance)
                var allDogs = await GetActiveDogsAsync();
                
                if (!allDogs.Success)
                    return allDogs;

                // For now, return all dogs since we don't have handler-dog relationships in the API
                // TODO: Add handler-dog relationship filtering
                return allDogs;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MSSA_Dog>>.ErrorResponse($"Error getting dogs: {ex.Message}");
            }
        }

        // Class operations
        public async Task<ApiResponse<List<MSSA_Class>>> GetActiveClassesAsync()
        {
            try
            {
                var classes = await _httpClient.GetFromJsonAsync<List<MSSA_Class>>(
                    $"/api/MSSA_Class?moduleid={_moduleId}");
                
                // Filter to active only
                var activeClasses = classes?.Where(c => c.IsActive).ToList() ?? new List<MSSA_Class>();
                
                // Sort by print order
                activeClasses = activeClasses.OrderBy(c => c.PrintOrder ?? 999).ToList();
                
                return ApiResponse<List<MSSA_Class>>.SuccessResponse(activeClasses);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MSSA_Class>>.ErrorResponse($"Error getting classes: {ex.Message}");
            }
        }

        // Health check
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
