using AppCode.Data;
using ToSic.Razor.Blade;
using ToSic.Sxc.Data;

namespace AppCode.Razor
{
  public abstract partial class AppRazor<TModel>: Custom.Hybrid.RazorTyped<TModel>
  {
    /// <summary>
    /// returns required attributes for editing mode if needed
    /// </summary>
    public IHtmlTag AttributesToEnableAddSpot(ImageWithHotspots hotspots)
    {
      if (!MyUser.IsContentAdmin) return null;

      var settings = AsStack<ImageWithHotspots>(hotspots, App.Settings);

      // Must wrap in Tag.Custom so the HTML-Encoding will be correct
      return Tag.RawHtml(
        // These three parameters are used by the JS to provide add-new-hotspot functionality
        // so you can click anywhere on the image to add a hotspot
        "data-module-id='" + MyContext.Module.Id
        + "' data-entity-id='" + hotspots.Id
        + "' data-guid='" + hotspots.Guid
        // These offsets are to position the marker on the image,
        // eg. if the marker is just a dot, it doesn't need an offset.
        // eg. if the marker is a map-pin, then the offset would be negative so the image starts higher up
        + "' data-iconoffset-x='" + settings.String("HotspotMarker.HotspotOffsetX")
        + "' data-iconoffset-y='" + settings.String("HotspotMarker.HotspotOffsetY") + "'"
      );
    }

    /// <summary>
    /// This position the marker on the image and sets the size as needed
    /// </summary>
    public IHtmlTag MarkerStyles(Hotspot hotspot, ITypedStack settings)
    {
      return Tag.RawHtml(
        "top: " + Kit.Convert.ForCode(hotspot.Y) + "%;"
        + " left: " + Kit.Convert.ForCode(hotspot.X) + "%;"
        + " width: " + settings.String("HotspotMarker.HotspotWidth") + ";"
        + " height: " + settings.String("HotspotMarker.HotspotHeight")
      );
    }

  }
}
