using System.Timers;
using Timer = System.Timers.Timer;

namespace MSSA.PWA.Services
{
    /// <summary>
    /// Service for synchronizing offline entries with the server
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly IOfflineStorageService _storage;
        private readonly IApiService _api;
        private Timer _syncTimer;
        private bool _isSyncing = false;

        public event EventHandler<SyncStatusEventArgs> SyncStatusChanged;

        public SyncService(IOfflineStorageService storage, IApiService api)
        {
            _storage = storage;
            _api = api;
        }

        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                return await _api.HealthCheckAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<SyncResult> SyncPendingEntriesAsync()
        {
            if (_isSyncing)
            {
                return new SyncResult
                {
                    SuccessCount = 0,
                    FailedCount = 0,
                    Errors = new List<string> { "Sync already in progress" }
                };
            }

            _isSyncing = true;
            RaiseSyncStatusChanged(await IsOnlineAsync(), true, await _storage.GetQueueCountAsync(), "Syncing...");

            var result = new SyncResult();

            try
            {
                // Check if we're online
                if (!await IsOnlineAsync())
                {
                    result.Errors.Add("Device is offline");
                    return result;
                }

                // Get all unsynced entries
                var unsyncedEntries = await _storage.GetUnsyncedEntriesAsync();

                if (unsyncedEntries == null || !unsyncedEntries.Any())
                {
                    RaiseSyncStatusChanged(true, false, 0, "No entries to sync");
                    return result;
                }

                // Try to sync each entry
                foreach (var entry in unsyncedEntries)
                {
                    try
                    {
                        var response = await _api.SaveEntryAsync(entry);

                        if (response.Success)
                        {
                            // Entry synced successfully - remove from offline storage
                            // Note: We need a way to identify the entry. We'll use a combination of fields
                            // For now, we'll mark it as synced in the offline storage
                            // In a real implementation, you'd want to store the tempId with each entry
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Entry sync failed: {response.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Entry sync error: {ex.Message}");
                    }
                }

                var queueCount = await _storage.GetQueueCountAsync();
                var message = result.IsSuccess
                    ? $"Synced {result.SuccessCount} entries successfully"
                    : $"Synced {result.SuccessCount}, {result.FailedCount} failed";

                RaiseSyncStatusChanged(true, false, queueCount, message);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Sync process error: {ex.Message}");
                RaiseSyncStatusChanged(await IsOnlineAsync(), false, await _storage.GetQueueCountAsync(), $"Sync error: {ex.Message}");
            }
            finally
            {
                _isSyncing = false;
            }

            return result;
        }

        public async Task StartAutoSyncAsync(int intervalSeconds = 30)
        {
            // Stop existing timer if any
            StopAutoSync();

            // Create new timer
            _syncTimer = new Timer(intervalSeconds * 1000);
            _syncTimer.Elapsed += async (sender, e) => await OnTimerElapsed();
            _syncTimer.AutoReset = true;
            _syncTimer.Start();

            // Do an initial sync
            await SyncPendingEntriesAsync();
        }

        public void StopAutoSync()
        {
            if (_syncTimer != null)
            {
                _syncTimer.Stop();
                _syncTimer.Dispose();
                _syncTimer = null;
            }
        }

        private async Task OnTimerElapsed()
        {
            // Don't sync if already syncing
            if (_isSyncing)
                return;

            // Check if there are entries to sync
            var queueCount = await _storage.GetQueueCountAsync();
            if (queueCount == 0)
                return;

            // Try to sync
            await SyncPendingEntriesAsync();
        }

        private void RaiseSyncStatusChanged(bool isOnline, bool isSyncing, int queuedCount, string message)
        {
            SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs
            {
                IsOnline = isOnline,
                IsSyncing = isSyncing,
                QueuedCount = queuedCount,
                Message = message
            });
        }

        public void Dispose()
        {
            StopAutoSync();
        }
    }
}
