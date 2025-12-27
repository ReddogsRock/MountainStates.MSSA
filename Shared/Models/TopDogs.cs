using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace MountainStates.MSSA.Module.TopDogs.Models
{
    [Table("MountainStates.MSSATopDogs")]
    public class TopDogs : ModelBase
    {
        [Key]
        public int TopDogsId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}
