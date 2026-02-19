namespace MountainStates.MSSA.Module.BackOfficeEntry.Models
{
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

                fullName = fullName
                    .Replace("On-Foot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("On Foot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("Onfoot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("  ", " ")
                    .Replace("- -", "-")
                    .Replace(" -", "-")
                    .Replace("- ", "-")
                    .Trim()
                    .Trim('-')
                    .Trim();

                return fullName;
            }
        }
    }
}
