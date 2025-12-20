using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace MountainStates.MSSA.Module.HelloWorld.Models
{
    [Table("MountainStates.MSSAHelloWorld")]
    public class HelloWorld : ModelBase
    {
        [Key]
        public int HelloWorldId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}
