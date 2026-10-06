using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests.Generation;

public class ButtonSettingsResolverTests
{
    private static EffectiveButtonSettings Resolve(
        WizardScreenButtonSettings? screen,
        WizardScreenButtonSettings? defaults,
        WizardButton button = WizardButton.Next)
        => ButtonSettingsResolver.Resolve(screen, defaults, button);

    [Fact]
    public void Nothing_set_gives_nothing_set()
    {
        var result = Resolve(new WizardScreenButtonSettings(), new WizardScreenButtonSettings());

        Assert.Equal(string.Empty, result.Caption);
        Assert.Null(result.Enabled);
        Assert.Null(result.Visible);
        Assert.Equal(string.Empty, result.FontFamily);
        Assert.Null(result.FontSize);
        Assert.Null(result.FontBold);
        Assert.Equal(string.Empty, result.Tooltip);
        Assert.Empty(result.CaptionByLanguage);
        Assert.Empty(result.TooltipByLanguage);
    }

    [Fact]
    public void Missing_settings_objects_count_as_nothing_set()
    {
        var result = Resolve(null, null);

        Assert.Equal(string.Empty, result.Caption);
        Assert.Null(result.Enabled);
        Assert.Empty(result.CaptionByLanguage);
    }

    [Fact]
    public void Own_text_wins_over_the_default_screen()
    {
        var screen = new WizardScreenButtonSettings { NextButtonCaption = "Start", NextButtonTooltip = "Ga door", NextButtonFontFamily = "Consolas" };
        var defaults = new WizardScreenButtonSettings { NextButtonCaption = "Verder", NextButtonTooltip = "Verder", NextButtonFontFamily = "Arial" };

        var result = Resolve(screen, defaults);

        Assert.Equal("Start", result.Caption);
        Assert.Equal("Ga door", result.Tooltip);
        Assert.Equal("Consolas", result.FontFamily);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_or_blank_own_text_falls_back_to_the_default_screen(string own)
    {
        var screen = new WizardScreenButtonSettings { NextButtonCaption = own, NextButtonTooltip = own, NextButtonFontFamily = own };
        var defaults = new WizardScreenButtonSettings { NextButtonCaption = "Verder", NextButtonTooltip = "Tip", NextButtonFontFamily = "Arial" };

        var result = Resolve(screen, defaults);

        Assert.Equal("Verder", result.Caption);
        Assert.Equal("Tip", result.Tooltip);
        Assert.Equal("Arial", result.FontFamily);
    }

    [Fact]
    public void Blank_default_text_gives_an_empty_result()
    {
        var defaults = new WizardScreenButtonSettings { NextButtonCaption = "   " };

        Assert.Equal(string.Empty, Resolve(new WizardScreenButtonSettings(), defaults).Caption);
    }

    [Fact]
    public void Own_value_wins_for_size_bold_enabled_and_visible_even_when_it_is_false()
    {
        var screen = new WizardScreenButtonSettings
        {
            NextButtonFontSize = 9, NextButtonFontBold = false, NextButtonEnabled = true, NextButtonVisible = true,
        };
        var defaults = new WizardScreenButtonSettings
        {
            NextButtonFontSize = 14, NextButtonFontBold = true, NextButtonEnabled = false, NextButtonVisible = false,
        };

        var result = Resolve(screen, defaults);

        Assert.Equal(9, result.FontSize);
        Assert.False(result.FontBold);
        Assert.True(result.Enabled);
        Assert.True(result.Visible);
    }

    [Fact]
    public void Unset_own_value_falls_back_to_the_default_screen_for_size_bold_enabled_and_visible()
    {
        var defaults = new WizardScreenButtonSettings
        {
            NextButtonFontSize = 14, NextButtonFontBold = true, NextButtonEnabled = false, NextButtonVisible = false,
        };

        var result = Resolve(new WizardScreenButtonSettings(), defaults);

        Assert.Equal(14, result.FontSize);
        Assert.True(result.FontBold);
        Assert.False(result.Enabled);
        Assert.False(result.Visible);
    }

    // ---- vertalingen per taal (docs/Ontwerp-Vertalingen-Standaardscherm.md) ------------------------

    // De vier rijen uit het voorbeeld in sectie 2 van het ontwerp: Standaardscherm met tekst "Verder"
    // en Nederlandse vertaling "Doorgaan", daarna wat het scherm zelf heeft. Duits heeft het
    // Standaardscherm niet, dus Duits krijgt nooit een eigen regel: de universele tekst geldt.
    [Theory]
    [InlineData("", "", "Verder", "Doorgaan")]
    [InlineData("Akkoord", "", "Akkoord", null)]
    [InlineData("Akkoord", "Ja", "Akkoord", "Ja")]
    [InlineData("", "Ja", "Verder", "Ja")]
    public void Translations_follow_the_cascade_of_the_design(string screenText, string screenDutch, string expectedUniversal, string? expectedDutch)
    {
        var defaults = new WizardScreenButtonSettings { NextButtonCaption = "Verder" };
        defaults.NextButtonCaptionByLanguage["dutch"] = "Doorgaan";
        var screen = new WizardScreenButtonSettings { NextButtonCaption = screenText };
        screen.NextButtonCaptionByLanguage["dutch"] = screenDutch;

        var result = Resolve(screen, defaults);

        Assert.Equal(expectedUniversal, result.Caption);
        if (expectedDutch is null)
        {
            Assert.Empty(result.CaptionByLanguage);
        }
        else
        {
            Assert.Equal(expectedDutch, Assert.Single(result.CaptionByLanguage).Value);
            Assert.Equal("dutch", Assert.Single(result.CaptionByLanguage).Key);
        }
    }

    [Fact]
    public void Languages_that_only_the_default_screen_has_are_included()
    {
        var defaults = new WizardScreenButtonSettings();
        defaults.NextButtonCaptionByLanguage["french"] = "Suivant";
        defaults.NextButtonTooltipByLanguage["french"] = "Continuer";

        var result = Resolve(new WizardScreenButtonSettings(), defaults);

        Assert.Equal(string.Empty, result.Caption);
        Assert.Equal("Suivant", result.CaptionByLanguage["french"]);
        Assert.Equal("Continuer", result.TooltipByLanguage["french"]);
    }

    [Fact]
    public void Blank_translations_do_not_count_on_the_screen_or_on_the_default_screen()
    {
        var screen = new WizardScreenButtonSettings();
        screen.NextButtonCaptionByLanguage["dutch"] = "  ";
        screen.NextButtonCaptionByLanguage["german"] = "";
        var defaults = new WizardScreenButtonSettings();
        defaults.NextButtonCaptionByLanguage["dutch"] = "Doorgaan";
        defaults.NextButtonCaptionByLanguage["german"] = "   ";
        defaults.NextButtonCaptionByLanguage["french"] = " ";

        var result = Resolve(screen, defaults);

        Assert.Equal(new[] { "dutch" }, result.CaptionByLanguage.Keys);
        Assert.Equal("Doorgaan", result.CaptionByLanguage["dutch"]);
    }

    [Fact]
    public void Caption_and_tooltip_cascade_separately()
    {
        var defaults = new WizardScreenButtonSettings();
        defaults.NextButtonCaptionByLanguage["dutch"] = "Doorgaan";
        defaults.NextButtonTooltipByLanguage["dutch"] = "Ga verder";
        var screen = new WizardScreenButtonSettings { NextButtonCaption = "Akkoord" };

        var result = Resolve(screen, defaults);

        Assert.Empty(result.CaptionByLanguage);
        Assert.Equal("Ga verder", result.TooltipByLanguage["dutch"]);
    }

    [Fact]
    public void Each_button_cascades_its_own_translations()
    {
        var defaults = new WizardScreenButtonSettings();
        defaults.BackButtonCaptionByLanguage["dutch"] = "Vorige";
        defaults.CancelButtonTooltipByLanguage["dutch"] = "Stoppen";

        var back = Resolve(new WizardScreenButtonSettings(), defaults, WizardButton.Back);
        var next = Resolve(new WizardScreenButtonSettings(), defaults, WizardButton.Next);
        var cancel = Resolve(new WizardScreenButtonSettings(), defaults, WizardButton.Cancel);

        Assert.Equal("Vorige", back.CaptionByLanguage["dutch"]);
        Assert.Empty(next.CaptionByLanguage);
        Assert.Empty(next.TooltipByLanguage);
        Assert.Equal("Stoppen", cancel.TooltipByLanguage["dutch"]);
        Assert.Empty(cancel.CaptionByLanguage);
    }

    [Theory]
    [InlineData("", "Ja", "Doorgaan", "Ja")]
    [InlineData("Akkoord", "Ja", "Doorgaan", "Ja")]
    [InlineData("Akkoord", "", "Doorgaan", "")]
    [InlineData("  ", "", "Doorgaan", "Doorgaan")]
    [InlineData("", "  ", "  ", "")]
    [InlineData(null, null, null, "")]
    public void ResolveTranslation_applies_the_three_rules_in_order(string? ownText, string? ownTranslation, string? defaultTranslation, string expected)
        => Assert.Equal(expected, ButtonSettingsResolver.ResolveTranslation(ownText, ownTranslation, defaultTranslation));

    [Theory]
    [InlineData(WizardButton.Back)]
    [InlineData(WizardButton.Next)]
    [InlineData(WizardButton.Cancel)]
    public void Each_button_reads_its_own_fields(WizardButton button)
    {
        var screen = new WizardScreenButtonSettings
        {
            BackButtonCaption = "back", BackButtonEnabled = false, BackButtonVisible = false,
            BackButtonFontFamily = "backfont", BackButtonFontSize = 11, BackButtonFontBold = true, BackButtonTooltip = "backtip",
            NextButtonCaption = "next", NextButtonEnabled = true, NextButtonVisible = true,
            NextButtonFontFamily = "nextfont", NextButtonFontSize = 12, NextButtonFontBold = false, NextButtonTooltip = "nexttip",
            CancelButtonCaption = "cancel", CancelButtonEnabled = null, CancelButtonVisible = false,
            CancelButtonFontFamily = "cancelfont", CancelButtonFontSize = 13, CancelButtonFontBold = true, CancelButtonTooltip = "canceltip",
        };
        screen.BackButtonCaptionByLanguage["dutch"] = "terug";
        screen.NextButtonCaptionByLanguage["dutch"] = "volgende";
        screen.CancelButtonCaptionByLanguage["dutch"] = "annuleren";
        screen.BackButtonTooltipByLanguage["dutch"] = "terugtip";
        screen.NextButtonTooltipByLanguage["dutch"] = "volgendetip";
        screen.CancelButtonTooltipByLanguage["dutch"] = "annulerentip";

        var result = Resolve(screen, null, button);

        switch (button)
        {
            case WizardButton.Back:
                Assert.Equal(("back", false, false, "backfont", 11, true, "backtip"), Tuple(result));
                Assert.Equal("terug", result.CaptionByLanguage["dutch"]);
                Assert.Equal("terugtip", result.TooltipByLanguage["dutch"]);
                break;
            case WizardButton.Next:
                Assert.Equal(("next", true, true, "nextfont", 12, false, "nexttip"), Tuple(result));
                Assert.Equal("volgende", result.CaptionByLanguage["dutch"]);
                Assert.Equal("volgendetip", result.TooltipByLanguage["dutch"]);
                break;
            default:
                Assert.Equal(("cancel", (bool?)null, false, "cancelfont", 13, true, "canceltip"), Tuple(result));
                Assert.Equal("annuleren", result.CaptionByLanguage["dutch"]);
                Assert.Equal("annulerentip", result.TooltipByLanguage["dutch"]);
                break;
        }
    }

    [Fact]
    public void Browse_button_has_no_default_screen_cascade()
    {
        var browse = new BrowseButtonSettings
        {
            Caption = "Zoeken...", Enabled = false, Visible = true, FontFamily = "Arial",
            FontSize = 10, FontBold = true, Tooltip = "Kies een map",
        };
        browse.CaptionByLanguage["dutch"] = "Zoek";
        browse.TooltipByLanguage["dutch"] = " ";

        var result = ButtonSettingsResolver.Resolve(browse);

        Assert.Equal(("Zoeken...", (bool?)false, (bool?)true, "Arial", (int?)10, (bool?)true, "Kies een map"), Tuple(result));
        Assert.Equal("Zoek", result.CaptionByLanguage["dutch"]);
        Assert.Empty(result.TooltipByLanguage);
    }

    [Fact]
    public void Missing_browse_settings_count_as_nothing_set()
    {
        var result = ButtonSettingsResolver.Resolve((BrowseButtonSettings?)null);

        Assert.Equal(string.Empty, result.Caption);
        Assert.Null(result.Enabled);
        Assert.Empty(result.CaptionByLanguage);
    }

    private static (string, bool?, bool?, string, int?, bool?, string) Tuple(EffectiveButtonSettings s)
        => (s.Caption, s.Enabled, s.Visible, s.FontFamily, s.FontSize, s.FontBold, s.Tooltip);
}
