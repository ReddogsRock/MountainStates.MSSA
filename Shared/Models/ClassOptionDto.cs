namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for class selection dropdown
    /// </summary>
    public class ClassOptionDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        
        public string DisplayName
        {
            get
            {
                var fullName = string.IsNullOrWhiteSpace(SubClassName) 
                    ? ClassName 
                    : $"{ClassName} - {SubClassName}";
                
                // Remove "On-Foot" variations since it's the inferred/default state
                // Keep "Horseback" in the name to distinguish it
                fullName = fullName
                    .Replace("On-Foot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("On Foot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("Onfoot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("  ", " ")  // Clean up double spaces
                    .Replace("- -", "-")  // Clean up double dashes
                    .Replace(" -", "-")   // Clean up space before dash
                    .Replace("- ", "-")   // Clean up space after dash
                    .Trim()
                    .Trim('-')            // Remove leading/trailing dashes
                    .Trim();              // Final trim
                
                return fullName;
            }
        }
    }
}
