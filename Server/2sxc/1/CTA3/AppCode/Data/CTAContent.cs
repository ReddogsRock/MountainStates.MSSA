namespace AppCode.Data
{
    public partial class CtaContent
    {
        // Add your own properties and methods here

        public new PresentationSettings Presentation => _presentation ??= As<PresentationSettings>(base.Presentation);
        private PresentationSettings _presentation;

    }
}
