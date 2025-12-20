using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Themes;
using Oqtane.Shared;

namespace MountainStates.MSSA.Theme.Brown
{
    public class ThemeInfo : ITheme
    {
        public Oqtane.Models.Theme Theme => new Oqtane.Models.Theme
        {
            Name = "MountainStates.MSSA Brown",
            Version = "1.0.0",
            PackageName = "MountainStates.MSSA.Theme.Brown",
            ThemeSettingsType = "MountainStates.MSSA.Theme.Brown.ThemeSettings, MountainStates.MSSA.Theme.Brown.Client.Oqtane",
            ContainerSettingsType = "MountainStates.MSSA.Theme.Brown.ContainerSettings, MountainStates.MSSA.Theme.Brown.Client.Oqtane",
            Resources = new List<Resource>()
            {
		// obtained from https://cdnjs.com/libraries
                new Stylesheet(Constants.BootstrapStylesheetUrl, Constants.BootstrapStylesheetIntegrity, "anonymous"),
                new Stylesheet("~/Theme.css"),
                new Script(Constants.BootstrapScriptUrl, Constants.BootstrapScriptIntegrity, "anonymous")
            }
        };

    }
}
