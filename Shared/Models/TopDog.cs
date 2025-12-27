using System;
using System.Collections.Generic;
using System.Text;

namespace MountainStates.MSSA.Module.TopDogs.Models
{
    public class TopDog
    {
        public int DogId { get; set; }
        public string Name { get; set; }
        public string? Breed { get; set; }  // Nullable
        public DateTime? DateOfBirth { get; set; }  // Nullable
        public string? OwnerName { get; set; }  // Nullable
        public string? Handler { get; set; }  // Nullable (already looks nullable in your data)
        public int Points { get; set; }
    }
}
