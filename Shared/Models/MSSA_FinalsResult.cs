using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Finals.Models
{
    public class MSSA_FinalsResult
    {
        public int Year { get; set; }

        [StringLength(50)]
        public string Level { get; set; }

        [StringLength(50)]
        public string Stock { get; set; }

        [StringLength(50)]
        public string Round { get; set; }

        public int? Place { get; set; }

        public int? HandlerId { get; set; }

        public int? DogId { get; set; }

        [StringLength(100)]
        public string HandlerName { get; set; }

        [StringLength(100)]
        public string DogName { get; set; }

        public decimal? TotalPoints { get; set; }

        public int? TotalTimeSeconds { get; set; }

        public int? RoundsCompleted { get; set; }

        public int? TotalRuns { get; set; }

        [StringLength(255)]
        public string SourceFile { get; set; }

        // Computed properties
        [NotMapped]
        public string Team => !string.IsNullOrEmpty(HandlerName) && !string.IsNullOrEmpty(DogName)
            ? $"{HandlerName} & {DogName}"
            : "";

        [NotMapped]
        public string DisplayTime
        {
            get
            {
                if (!TotalTimeSeconds.HasValue) return "";
                var minutes = TotalTimeSeconds.Value / 60;
                var seconds = TotalTimeSeconds.Value % 60;
                return $"{minutes}:{seconds:D2}";
            }
        }

        [NotMapped]
        public bool IsUltimate => Stock == "Ultimate";
    }
}
