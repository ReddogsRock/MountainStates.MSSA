
using AppCode.Data;
using Custom.Data;
using ToSic.Sxc.Context;

namespace AppCode.Razor
{
  public abstract class CtaSplit: AppRazor
  {
    public CtaContent Cta => _cta ??= As<CtaContent>(MyItem);
    private CtaContent _cta;

    public new ICmsView<TextImageViewSettings, CustomItem> MyView => _myView ??= Customize.MyView<TextImageViewSettings, CustomItem>();
    private ICmsView<TextImageViewSettings, CustomItem> _myView;

    public bool ShowTextFirst => _showTextFirst ??= MyView.Settings.TextFirst;
    private bool? _showTextFirst;

    public int TextCols => _colsText ??= ShowTextFirst ? MyView.Settings.ColsElement : 12 - MyView.Settings.ColsElement;
    private int? _colsText;

    public int ImageCols => _imageCols ??= 12 - TextCols;
    private int? _imageCols;
  }
}