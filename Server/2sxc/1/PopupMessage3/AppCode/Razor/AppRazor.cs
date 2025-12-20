using AppCode.Data;
using ToSic.Sxc.Apps;

namespace AppCode.Razor
{
  /// <summary>
  /// Base Class for Razor Views which have a typed App and a typed Model
  /// </summary>
  public abstract partial class AppRazor<TModel>: Custom.Hybrid.RazorTyped<TModel>
  {
    public string DomId => "app-popup3-js-modal-" + UniqueKey;

    public Popup PopUp => _popup ??= As<Popup>(MyItem);
    private Popup _popup;
  }
}
