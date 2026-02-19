using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Models
{
    /// <summary>
    /// DTO for creating or updating a result entry (back office scenario).
    /// EntryId = 0 means create new; EntryId > 0 means update existing.
    /// </summary>
    public class SaveResultEntryDto
    {
        public int EntryId { get; set; }   // 0 = new entry

        [Required]
        public int TrialId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int HandlerId { get; set; }

        [Required]
        public int DogId { get; set; }

        // Timing stored as "MM:SS.ff" strings to avoid System.Text.Json TimeSpan issues
        public string RunTimeStr { get; set; }
        public string TieBreakerTimeStr { get; set; }

        // Obstacle scores (up to 9)
        public decimal? ObstacleScore1 { get; set; }
        public decimal? ObstacleScore2 { get; set; }
        public decimal? ObstacleScore3 { get; set; }
        public decimal? ObstacleScore4 { get; set; }
        public decimal? ObstacleScore5 { get; set; }
        public decimal? ObstacleScore6 { get; set; }
        public decimal? ObstacleScore7 { get; set; }
        public decimal? ObstacleScore8 { get; set; }
        public decimal? ObstacleScore9 { get; set; }

        public decimal? Penalty { get; set; }
        public string Comments { get; set; }
    }
}
