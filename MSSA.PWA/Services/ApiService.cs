using System.Net.Http.Json;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MSSA.PWA.Services
{
    /// <summary>
    /// Service for making API calls to the Oqtane backend
    /// NOTE: This is a stub implementation. Update the base URL and endpoints
    /// to match your actual Oqtane API structure.
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // TODO: Update this with your actual Oqtane API base URL
            _baseApiUrl = configuration["ApiBaseUrl"] ?? "https://your-oqtane-site.com/api";
        }

        // Entry operations
        public async Task<ApiResponse<MSSA_Entry>> SaveEntryAsync(MSSA_Entry entry)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/entries", entry);

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
                var entries = await _httpClient.GetFromJsonAsync<List<MSSA_Entry>>(
                    $"{_baseApiUrl}/entries/trial/{trialId}");

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
                    $"{_baseApiUrl}/trials/{trialId}");

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
                var trials = await _httpClient.GetFromJsonAsync<List<MSSA_Trial>>(
                    $"{_baseApiUrl}/trials/active");

                return ApiResponse<List<MSSA_Trial>>.SuccessResponse(trials ?? new List<MSSA_Trial>());
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
                var handlers = await _httpClient.GetFromJsonAsync<List<MSSA_Handler>>(
                    $"{_baseApiUrl}/handlers/active");

                return ApiResponse<List<MSSA_Handler>>.SuccessResponse(handlers ?? new List<MSSA_Handler>());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MSSA_Handler>>.ErrorResponse($"Error getting handlers: {ex.Message}");
            }
        }

        public async Task<ApiResponse<MSSA_Handler>> GetHandlerAsync(int handlerId)
        {
            try
            {
                var handler = await _httpClient.GetFromJsonAsync<MSSA_Handler>(
                    $"{_baseApiUrl}/handlers/{handlerId}");

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
                    $"{_baseApiUrl}/dogs/active");

                return ApiResponse<List<MSSA_Dog>>.SuccessResponse(dogs ?? new List<MSSA_Dog>());
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
                var dogs = await _httpClient.GetFromJsonAsync<List<MSSA_Dog>>(
                    $"{_baseApiUrl}/dogs/handler/{handlerId}");

                return ApiResponse<List<MSSA_Dog>>.SuccessResponse(dogs ?? new List<MSSA_Dog>());
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
                    $"{_baseApiUrl}/classes/active");

                return ApiResponse<List<MSSA_Class>>.SuccessResponse(classes ?? new List<MSSA_Class>());
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
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}