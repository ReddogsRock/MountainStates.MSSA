using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Finals.Models
{
    /// <summary>
    /// Represents the MSSA_Finals table (renamed from Staging_FinalsResults)
    /// Contains imported Finals results data
    /// </summary>
    [Table("MSSA_Finals")]
    public class MSSA_Finals
    {
        [Key]
        public int FinalsResultId { get; set; }

        // Metadata from folder structure
        public int? Year { get; set; }

        [StringLength(50)]
        public string Level { get; set; }

        [StringLength(50)]
        public string Stock { get; set; }

        [StringLength(50)]
        public string Round { get; set; }

        [StringLength(255)]
        public string SourceFile { get; set; }

        // Excel columns
        public int? Place { get; set; }

        [StringLength(200)]
        public string Team { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Pts { get; set; }

        [StringLength(20)]
        public string Time { get; set; }

        public int? TimeSeconds { get; set; }

        // Parsed data
        [StringLength(100)]
        public string HandlerName { get; set; }

        [StringLength(100)]
        public string DogName { get; set; }

        public int? HandlerId { get; set; }

        public int? DogId { get; set; }

        [StringLength(50)]
        public string MatchStatus { get; set; }

        // Import tracking
        public DateTime ImportedDate { get; set; }
    }
}
