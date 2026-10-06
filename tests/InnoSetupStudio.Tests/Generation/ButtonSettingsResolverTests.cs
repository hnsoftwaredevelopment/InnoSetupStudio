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

    [Fact]
    public void Translations_come_from_the_screen_only_and_skip_blank_values()
    {
        var screen = new WizardScreenButtonSettings();
        screen.NextButtonCaptionByLanguage["dutch"] = "Volgende";
        screen.NextButtonCaptionByLanguage["german"] = "  ";
        screen.NextButtonTooltipByLanguage["dutch"] = "Ga verder";
        var defaults = new WizardScreenButtonSettings();
        defaults.NextButtonCaptionByLanguage["french"] = "Suivant";
        defaults.NextButtonTooltipByLanguage["french"] = "Continuer";

        var result = Resolve(screen, defaults);

        Assert.Equal(new[] { "dutch" }, result.CaptionByLanguage.Keys);
        Assert.Equal("Volgende", result.CaptionByLanguage["dutch"]);
        Assert.Equal(new[] { "dutch" }, result.TooltipByLanguage.Keys);
    }

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
