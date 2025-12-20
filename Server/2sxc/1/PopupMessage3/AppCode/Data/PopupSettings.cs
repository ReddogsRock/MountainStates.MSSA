namespace AppCode.Data
{
  public partial class PopupSettings
  {
    public bool PopupIsActiveAndWithinDateRange() => Activated == true    // popup activated?
      && ActiveFrom <= System.DateTime.Now.Date    // showFrom <= today?
      && ActiveTo >= System.DateTime.Now.Date;     // showTo >= today?
  }
}