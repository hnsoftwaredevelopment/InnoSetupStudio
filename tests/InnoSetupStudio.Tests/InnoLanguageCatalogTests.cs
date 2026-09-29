using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests;

public class InnoLanguageCatalogTests
{
    [Fact]
    public void LanguagesStartsWithBuiltInEnglish()
    {
        var first = InnoLanguageCatalog.Languages[0];

        Assert.Equal(InnoLanguageCatalog.EnglishId, first.Id);
        Assert.True(first.IsBuiltIn);
    }

    [Fact]
    public void LanguagesContainsThirtyThreeEntries()
    {
        // Engels (ingebouwd) plus de 32 .isl-bestanden uit Herberts Languages-map, zie
        // InnoLanguageCatalog voor de volledige lijst.
        Assert.Equal(33, InnoLanguageCatalog.Languages.Count);
    }

    [Fact]
    public void LanguageIdsAreUniqueAndLowercase()
    {
        var ids = InnoLanguageCatalog.Languages.Select(l => l.Id).ToList();

        Assert.Equal(ids.Distinct(), ids);
        Assert.All(ids, id => Assert.Equal(id, id.ToLowerInvariant()));
    }

    [Fact]
    public void OnlyEnglishIsBuiltIn()
    {
        Assert.All(
            InnoLanguageCatalog.Languages.Where(l => l.Id != InnoLanguageCatalog.EnglishId),
            l => Assert.False(l.IsBuiltIn));
    }
}
