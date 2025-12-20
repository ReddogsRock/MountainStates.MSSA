using AppCode.Data;
using System.Collections.Generic;
using System.Linq;
using ToSic.Razor.Blade;

namespace AppCode.Razor
{
  /// <summary>
  /// Base Class for Razor Views which have a typed App and a typed Model
  /// </summary>
  public abstract partial class AppRazor<TModel>
  {

    /// <summary>
    /// Get all terms grouped by the first character of their title
    /// </summary>
    public List<IGrouping<string, Term>> TermsGroupedByFirstCharacter()
      => App.Data.GetAll<Term>()
        .GroupBy(l => FirstCharOrDash(l.Title))
        .OrderBy(s => s.Key)
        .ToList();

    /// <summary>
    /// Get the first character of a string or a dash if the string is empty
    /// </summary>
    public string FirstCharOrDash(string original) 
      => Text.First(original, "-").Substring(0, 1).ToUpper();

    /// <summary>
    /// Get the title of a term with its abbreviation (if there is one)
    /// </summary>
    public string TitleWithAbbreviation(Term term)
    {
      var titleAbbreviation = term.Abbreviation;
      var separator = Text.Has(titleAbbreviation) ? " – " : "";
      return titleAbbreviation + separator + term.Title;
    }
  }
}
