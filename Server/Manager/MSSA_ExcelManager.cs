using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Entries.Manager;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;

namespace MountainStates.MSSA.Module.MSSA_Excel.Manager
{
    public class MSSA_ExcelManager : IMSSA_ExcelManager, ITransientService
    {
        private readonly IMSSA_EntryManager _entryManager;
        private readonly IMSSA_EventManager _eventManager;
        private readonly IMSSA_HandlerManager _handlerManager;
        private readonly IMSSA_DogManager _dogManager;

        public MSSA_ExcelManager(
            IMSSA_EntryManager entryManager,
            IMSSA_EventManager eventManager,
            IMSSA_HandlerManager handlerManager,
            IMSSA_DogManager dogManager)
        {
            _entryManager = entryManager;
            _eventManager = eventManager;
            _handlerManager = handlerManager;
            _dogManager = dogManager;

            // Set EPPlus license context (for EPPlus 7.x)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Workflow 1: Import Entries

        public async Task<ExcelImportResult> ImportEntriesAsync(byte[] fileData, int moduleId, int userId)
        {
            var result = new ExcelImportResult();

            try
            {
                using (var stream = new MemoryStream(fileData))
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // First sheet
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount < 2)
                    {
                        result.Success = false;
                        result.Errors.Add("Excel file is empty or has no data rows");
                        return result;
                    }

                    // Expected columns: TrialId, HandlerId, DogId, ClassId
                    // Row 1 is headers, data starts at row 2

                    // Load all entries once for duplicate checking
                    var allEntries = await _entryManager.GetEntriesAsync(moduleId);

                    for (int row = 2; row <= rowCount; row++)
                    {
                        result.RowsProcessed++;

                        try
                        {
                            var trialId = GetIntValue(worksheet, row, 1);
                            var handlerId = GetIntValue(worksheet, row, 2);
                            var dogId = GetIntValue(worksheet, row, 3);
                            var classId = GetIntValue(worksheet, row, 4);

                            if (trialId == 0 || handlerId == 0 || dogId == 0 || classId == 0)
                            {
                                result.Warnings.Add($"Row {row}: Missing required fields (TrialId, HandlerId, DogId, or ClassId)");
                                result.RowsSkipped++;
                                continue;
                            }

                            // Check if entry already exists
                            var existingEntry = allEntries.FirstOrDefault(e => 
                                e.TrialId == trialId && 
                                e.HandlerId == handlerId && 
                                e.DogId == dogId && 
                                e.ClassId == classId);
                            
                            if (existingEntry != null)
                            {
                                result.Warnings.Add($"Row {row}: Entry already exists for Trial {trialId}, Handler {handlerId}, Dog {dogId}, Class {classId}");
                                result.RowsSkipped++;
                                continue;
                            }

                            // Create new entry
                            var entry = new MSSA_Entry
                            {
                                TrialId = trialId,
                                HandlerId = handlerId,
                                DogId = dogId,
                                ClassId = classId,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                EnteredBy = userId,
                                ModifiedBy = userId
                            };

                            await _entryManager.AddEntryAsync(entry, moduleId);
                            result.RowsInserted++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Row {row}: {ex.Message}");
                        }
                    }

                    result.Success = result.RowsInserted > 0;
                    result.Message = $"Processed {result.RowsProcessed} rows: {result.RowsInserted} inserted, {result.RowsSkipped} skipped";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error processing file: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Workflow 2: Generate Run Order

        public async Task<byte[]> GenerateRunOrderAsync(int trialId, int moduleId)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Run Order");

                // Get trial info
                var trial = await _eventManager.GetTrialAsync(trialId, moduleId);
                
                // Get all entries and filter by trial
                var allEntries = await _entryManager.GetEntriesAsync(moduleId);
                var entries = allEntries.Where(e => e.TrialId == trialId).ToList();
                
                // Get reference data
                var classes = await _entryManager.GetClassesAsync(moduleId);
                var handlers = await _handlerManager.GetHandlersAsync(moduleId);
                var dogs = await _dogManager.GetDogsAsync(moduleId);

                // Add header info
                worksheet.Cells[1, 1].Value = "Trial:";
                worksheet.Cells[1, 2].Value = trial.TrialName;
                worksheet.Cells[2, 1].Value = "Date:";
                worksheet.Cells[2, 2].Value = trial.TrialDate.ToShortDateString();
                worksheet.Cells[3, 1].Value = "Stock:";
                worksheet.Cells[3, 2].Value = trial.Stock;

                // Column headers (starting at row 5)
                int headerRow = 5;
                worksheet.Cells[headerRow, 1].Value = "EntryId";
                worksheet.Cells[headerRow, 2].Value = "Class";
                worksheet.Cells[headerRow, 3].Value = "Handler ID";
                worksheet.Cells[headerRow, 4].Value = "Handler Name";
                worksheet.Cells[headerRow, 5].Value = "Dog ID";
                worksheet.Cells[headerRow, 6].Value = "Dog Name";
                worksheet.Cells[headerRow, 7].Value = "Run Order";
                worksheet.Cells[headerRow, 8].Value = "Time";
                worksheet.Cells[headerRow, 9].Value = "Tie Time";
                worksheet.Cells[headerRow, 10].Value = "O1";
                worksheet.Cells[headerRow, 11].Value = "O2";
                worksheet.Cells[headerRow, 12].Value = "O3";
                worksheet.Cells[headerRow, 13].Value = "O4";
                worksheet.Cells[headerRow, 14].Value = "O5";
                worksheet.Cells[headerRow, 15].Value = "O6";
                worksheet.Cells[headerRow, 16].Value = "O7";
                worksheet.Cells[headerRow, 17].Value = "O8";
                worksheet.Cells[headerRow, 18].Value = "O9";
                worksheet.Cells[headerRow, 19].Value = "Penalty";
                worksheet.Cells[headerRow, 20].Value = "Course Points";
                worksheet.Cells[headerRow, 21].Value = "Place";

                // Format header
                using (var range = worksheet.Cells[headerRow, 1, headerRow, 21])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Group entries by class and randomize within each class
                var random = new Random();
                int currentRow = headerRow + 1;

                foreach (var classGroup in entries.GroupBy(e => e.ClassId).OrderBy(g => g.Key))
                {
                    var classInfo = classes.FirstOrDefault(c => c.ClassId == classGroup.Key);
                    var classEntries = classGroup.OrderBy(x => random.Next()).ToList();

                    int runOrder = 1;
                    foreach (var entry in classEntries)
                    {
                        // Look up handler and dog info
                        var handler = handlers.FirstOrDefault(h => h.HandlerId == entry.HandlerId);
                        var dog = dogs.FirstOrDefault(d => d.DogId == entry.DogId);

                        worksheet.Cells[currentRow, 1].Value = entry.EntryId;
                        worksheet.Cells[currentRow, 2].Value = $"{classInfo?.ClassName} {classInfo?.SubClassName}".Trim();
                        worksheet.Cells[currentRow, 3].Value = entry.HandlerId;
                        worksheet.Cells[currentRow, 4].Value = handler?.FullName ?? "";
                        worksheet.Cells[currentRow, 5].Value = entry.DogId;
                        worksheet.Cells[currentRow, 6].Value = dog?.Name ?? "";
                        worksheet.Cells[currentRow, 7].Value = runOrder++;

                        // Leave score columns blank for scorekeeper to fill in
                        // Columns 8-21 are blank

                        currentRow++;
                    }

                    // Add blank row between classes
                    currentRow++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        #endregion

        #region Workflow 3: Import Scores

        public async Task<ExcelImportResult> ImportScoresAsync(byte[] fileData, int moduleId, int userId)
        {
            var result = new ExcelImportResult();

            try
            {
                using (var stream = new MemoryStream(fileData))
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    // Find header row (should be row 5 based on run order template)
                    int headerRow = 5;
                    int dataStartRow = headerRow + 1;

                    for (int row = dataStartRow; row <= rowCount; row++)
                    {
                        // Skip blank rows
                        var entryId = GetIntValue(worksheet, row, 1);
                        if (entryId == 0) continue;

                        result.RowsProcessed++;

                        try
                        {
                            // Get existing entry
                            var entry = await _entryManager.GetEntryAsync(entryId, moduleId);
                            if (entry == null)
                            {
                                result.Warnings.Add($"Row {row}: Entry {entryId} not found");
                                result.RowsSkipped++;
                                continue;
                            }

                            // Parse times (format: "245" -> 2:45)
                            var timeValue = GetStringValue(worksheet, row, 8);
                            var tieTimeValue = GetStringValue(worksheet, row, 9);

                            entry.RunTime = ParseTime(timeValue);
                            entry.TieBreakerTime = ParseTime(tieTimeValue);

                            // Parse obstacle scores
                            entry.ObstacleScore1 = GetDecimalValue(worksheet, row, 10);
                            entry.ObstacleScore2 = GetDecimalValue(worksheet, row, 11);
                            entry.ObstacleScore3 = GetDecimalValue(worksheet, row, 12);
                            entry.ObstacleScore4 = GetDecimalValue(worksheet, row, 13);
                            entry.ObstacleScore5 = GetDecimalValue(worksheet, row, 14);
                            entry.ObstacleScore6 = GetDecimalValue(worksheet, row, 15);
                            entry.ObstacleScore7 = GetDecimalValue(worksheet, row, 16);
                            entry.ObstacleScore8 = GetDecimalValue(worksheet, row, 17);
                            entry.ObstacleScore9 = GetDecimalValue(worksheet, row, 18);
                            entry.Penalty = GetDecimalValue(worksheet, row, 19);

                            // Course Points is calculated automatically via TotalScore property
                            // We'll store it in TrialPoints for now
                            entry.TrialPoints = (int?)entry.TotalScore;

                            entry.ModifiedDate = DateTime.Now;
                            entry.ModifiedBy = userId;

                            await _entryManager.UpdateEntryAsync(entry, moduleId);
                            result.RowsUpdated++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Row {row}: {ex.Message}");
                        }
                    }

                    // Now calculate placing and points for all entries in this trial
                    await CalculatePlacingAndPointsAsync(worksheet, headerRow, dataStartRow, rowCount, moduleId, userId);

                    result.Success = result.RowsUpdated > 0;
                    result.Message = $"Processed {result.RowsProcessed} rows: {result.RowsUpdated} updated, placing and points calculated";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error processing file: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private int GetIntValue(ExcelWorksheet worksheet, int row, int col)
        {
            var value = worksheet.Cells[row, col].Value;
            if (value == null) return 0;
            if (int.TryParse(value.ToString(), out int result))
                return result;
            return 0;
        }

        private decimal? GetDecimalValue(ExcelWorksheet worksheet, int row, int col)
        {
            var value = worksheet.Cells[row, col].Value;
            if (value == null) return null;
            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;
            return null;
        }

        private string GetStringValue(ExcelWorksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Value?.ToString() ?? string.Empty;
        }

        private TimeSpan? ParseTime(string timeValue)
        {
            if (string.IsNullOrWhiteSpace(timeValue)) return null;

            // Remove any non-digit characters
            var digits = new string(timeValue.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits)) return null;

            // Parse based on length
            // "245" -> 2:45 (2 minutes 45 seconds)
            // "12" -> 0:12 (12 seconds)
            // "1234" -> 12:34 (12 minutes 34 seconds)

            if (digits.Length == 1 || digits.Length == 2)
            {
                // Just seconds
                if (int.TryParse(digits, out int seconds))
                    return new TimeSpan(0, 0, seconds);
            }
            else if (digits.Length == 3)
            {
                // M:SS
                var minutes = int.Parse(digits.Substring(0, 1));
                var seconds = int.Parse(digits.Substring(1, 2));
                return new TimeSpan(0, minutes, seconds);
            }
            else if (digits.Length == 4)
            {
                // MM:SS
                var minutes = int.Parse(digits.Substring(0, 2));
                var seconds = int.Parse(digits.Substring(2, 2));
                return new TimeSpan(0, minutes, seconds);
            }
            else if (digits.Length >= 5)
            {
                // MMM:SS or longer
                var minutes = int.Parse(digits.Substring(0, digits.Length - 2));
                var seconds = int.Parse(digits.Substring(digits.Length - 2, 2));
                return new TimeSpan(0, minutes, seconds);
            }

            return null;
        }

        private async Task CalculatePlacingAndPointsAsync(ExcelWorksheet worksheet, int headerRow, int dataStartRow, int rowCount, int moduleId, int userId)
        {
            // Get all entries from the worksheet and group by trial and class
            var entries = new List<MSSA_Entry>();
            
            for (int row = dataStartRow; row <= rowCount; row++)
            {
                var entryId = GetIntValue(worksheet, row, 1);
                if (entryId == 0) continue;

                var entry = await _entryManager.GetEntryAsync(entryId, moduleId);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            // Group by Trial and Class
            var groupedEntries = entries
                .GroupBy(e => new { e.TrialId, e.ClassId })
                .ToList();

            foreach (var group in groupedEntries)
            {
                // Sort by Course Points (descending), then by Time (ascending)
                var sortedEntries = group
                    .OrderByDescending(e => e.TotalScore ?? 0)
                    .ThenBy(e => e.RunTime ?? TimeSpan.MaxValue)
                    .ToList();

                // Calculate total entries in this class for points calculation
                int totalEntriesInClass = sortedEntries.Count;
                int maxPoints = totalEntriesInClass * 30;

                // Assign placing and calculate points
                int currentPlace = 1;
                decimal? lastScore = null;
                TimeSpan? lastTime = null;

                for (int i = 0; i < sortedEntries.Count; i++)
                {
                    var entry = sortedEntries[i];

                    // Determine if this is a tie (same score and same time)
                    if (i > 0 && entry.TotalScore == lastScore && entry.RunTime == lastTime)
                    {
                        // Tie - same place as previous
                        entry.Placing = currentPlace;
                    }
                    else
                    {
                        // New place
                        currentPlace = i + 1;
                        entry.Placing = currentPlace;
                    }

                    // Calculate points: (EntryCount × 30) - ((Place - 1) × 100)
                    int calculatedPoints = maxPoints - ((entry.Placing.Value - 1) * 100);
                    
                    // Ensure points don't go negative
                    entry.TrialPoints = calculatedPoints > 0 ? calculatedPoints : 0;

                    // Update last score and time for tie detection
                    lastScore = entry.TotalScore;
                    lastTime = entry.RunTime;

                    // Update entry in database
                    entry.ModifiedDate = DateTime.Now;
                    entry.ModifiedBy = userId;
                    await _entryManager.UpdateEntryAsync(entry, moduleId);
                }
            }
        }

        #endregion
    }
}
