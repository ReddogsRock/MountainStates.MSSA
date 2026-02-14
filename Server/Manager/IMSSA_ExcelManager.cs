using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Models;

namespace MountainStates.MSSA.Module.MSSA_Excel.Manager
{
    public interface IMSSA_ExcelManager
    {
        // Entry Import (Workflow 1)
        Task<ExcelImportResult> ImportEntriesAsync(byte[] fileData, int moduleId, int userId);

        // Run Order Generation (Workflow 2)
        Task<byte[]> GenerateRunOrderAsync(int trialId, int moduleId);

        // Score Import (Workflow 3)
        Task<ExcelImportResult> ImportScoresAsync(byte[] fileData, int moduleId, int userId);
    }

    public class ExcelImportResult
    {
        public bool Success { get; set; }
        public int RowsProcessed { get; set; }
        public int RowsInserted { get; set; }
        public int RowsUpdated { get; set; }
        public int RowsSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string Message { get; set; }
    }
}
