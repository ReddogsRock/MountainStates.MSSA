namespace MSSA.PWA.Services
{
    /// <summary>
    /// Service for synchronizing offline entries with the server
    /// </summary>
    public interface ISyncService
    {
        // Connection status
        Task<bool> IsOnlineAsync();

        // Sync operations
        Task<SyncResult> SyncPendingEntriesAsync();

        // Auto-sync management
        Task StartAutoSyncAsync(int intervalSeconds = 30);
        void StopAutoSync();

        // Events for status updates
        event EventHandler<SyncStatusEventArgs> SyncStatusChanged;
    }

    /// <summary>
    /// Result of a sync operation
    /// </summary>
    public class SyncResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => FailedCount == 0;
    }

    /// <summary>
    /// Event args for sync status changes
    /// </summary>
    public class SyncStatusEventArgs : EventArgs
    {
        public bool IsOnline { get; set; }
        public bool IsSyncing { get; set; }
        public int QueuedCount { get; set; }
        public string Message { get; set; }
    }
}
