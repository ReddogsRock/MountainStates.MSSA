using System;

namespace MountainStates.MSSA.Module.MSSA_Finals.Models
{
    /// <summary>
    /// Represents the individual round results that make up an Ultimate standing
    /// </summary>
    public class MSSA_UltimateBreakdown
    {
        public int Year { get; set; }
        public string Level { get; set; }
        public int? HandlerId { get; set; }
        public int? DogId { get; set; }
        public string HandlerName { get; set; }
        public string DogName { get; set; }
        public string Stock { get; set; }
        public string Round { get; set; }
        public int? Place { get; set; }
        public decimal? Pts { get; set; }
        public int? TimeSeconds { get; set; }
        public string SourceFile { get; set; }

        public string DisplayTime
        {
            get
            {
                if (!TimeSeconds.HasValue) return "";
                var minutes = TimeSeconds.Value / 60;
                var seconds = TimeSeconds.Value % 60;
                return $"{minutes}:{seconds:D2}";
            }
        }
    }
}
