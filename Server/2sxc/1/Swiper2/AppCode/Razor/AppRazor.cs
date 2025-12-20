using AppCode.Data;

namespace AppCode.Razor
{
  public abstract partial class AppRazor<TModel> : Custom.Hybrid.RazorTyped<TModel>
  {

    /// <summary>
    /// Generate class names for the overlay div, based on the settings of the slide
    /// </summary>
    public string OverlayAlignClasses(string textPosition)
    {
      var pos = textPosition ?? "";
      if (pos.StartsWith("c")) return "align-items-center";   // center: cl, cc, cr
      if (pos.StartsWith("b")) return "align-items-end";      // bottom: bl, bc, br
      return "";
    }

    /// <summary>
    /// Generate bootstrap4 css class names for the overlay div, based on the settings of the slide
    /// </summary>
    public string OverlayTextAlignClasses(string textPosition)
    {
      var pos = textPosition ?? "";
      if (pos.EndsWith("c")) return "text-center";    // center: tc, cc, bc
      if (pos.EndsWith("r")) return Kit.Css.Is("bs4") // right:  tr, cr, br
        ? "text-right"  // Bootstrap 4
        : "text-end";   // Bootstrap 5 / other
      return "";
    }

    /// <summary>
    /// Generate custom css class names for the overlay div, based on the settings of the slide
    /// This changes the effects as well as background gradients
    /// </summary>
    public string SlideWrapperClasses(Slide settings)
    {
      return "content-position-" + (settings.TextPosition ?? "none")
        + " content-effect-" + (settings.OverlayEffect ?? "none")
        + " " + (settings.DarkContent ? "dark" : "light") + "-content";
    }

  }
}
