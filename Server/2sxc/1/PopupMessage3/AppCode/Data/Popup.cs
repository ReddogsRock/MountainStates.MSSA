namespace AppCode.Data
{
    public partial class Popup
    {
        /// <summary>
        /// The presentation - typed
        /// </summary>
        public new PopupSettings Presentation => _presentation ??= As<PopupSettings>(base.Presentation);
        private PopupSettings _presentation;

    }
}
