namespace AppCode.Data
{
  public partial class Album
  {
    // Add your own properties and methods here

    public new AlbumPresentation Presentation => _presentation ??= As<AlbumPresentation>(base.Presentation);
    private AlbumPresentation _presentation;
  }
}
