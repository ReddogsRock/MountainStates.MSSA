using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MSSA.PWA.Services
{
    /// <summary>
    /// Service for making API calls to the Oqtane backend
    /// </summary>
    public interface IApiService
    {
        // Entry operations
        Task<ApiResponse<MSSA_Entry>> SaveEntryAsync(MSSA_Entry entry);
        Task<ApiResponse<List<MSSA_Entry>>> GetEntriesByTrialAsync(int trialId);

        // Trial operations
        Task<ApiResponse<MSSA_Trial>> GetTrialAsync(int trialId);
        Task<ApiResponse<List<MSSA_Trial>>> GetActiveTrialsAsync();

        // Handler operations
        Task<ApiResponse<List<MSSA_Handler>>> GetActiveHandlersAsync();
        Task<ApiResponse<MSSA_Handler>> GetHandlerAsync(int handlerId);

        // Dog operations
        Task<ApiResponse<List<MSSA_Dog>>> GetActiveDogsAsync();
        Task<ApiResponse<List<MSSA_Dog>>> GetDogsByHandlerAsync(int handlerId);

        // Class operations
        Task<ApiResponse<List<MSSA_Class>>> GetActiveClassesAsync();

        // Health check
        Task<bool> HealthCheckAsync();
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResponse(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 500)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = statusCode
            };
        }
    }
}
