using System.Linq;
using System.Collections.Generic;
using ToSic.Razor.Blade;
using ToSic.Sxc.Data;

public class Logic: Custom.Hybrid.CodeTyped
{
  public string FirstCharOrDash(string original)
  {
    return Text.First(original, "-").Substring(0, 1).ToUpper();
  }

  public List<IGrouping<string, ITypedItem>> TermsGroupedByFirstCharacter()
  {
    return AsItems(App.Data["Term"])
      .GroupBy(l => FirstCharOrDash(l.String("Title")))
      .OrderBy(s => s.Key)
      .ToList();
  }

  public string TitleWithAbbreviation(ITypedItem term)
  {
    var titleAbbreviation = term.String("Abbreviation");
    titleAbbreviation = (Text.Has(titleAbbreviation) ? titleAbbreviation + " – " : "");
    var fullTermTitle = titleAbbreviation + term.String("Title");
    return fullTermTitle;
  }
}