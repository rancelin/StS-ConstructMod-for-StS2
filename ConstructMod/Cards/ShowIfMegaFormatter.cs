using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

/// <summary>
/// A SmartFormat formatter for {IfMega:mega:megaText|normalText} — the mega-upgrade analogue of
/// vanilla's ShowIfUpgradedFormatter for {IfUpgraded:show:...}. Named "mega" (SmartFormat rejects
/// duplicate formatter names, so it cannot share the vanilla "show" name). Registered
/// automatically via BaseLib's <see cref="IAutoRegisterFormatSpecifier"/>.
/// </summary>
public class ShowIfMegaFormatter : IFormatter, IAutoRegisterFormatSpecifier
{
    // NOT "show" — SmartFormat throws ArgumentException on registering a formatter whose name
    // already exists (the vanilla ShowIfUpgradedFormatter owns "show").
    public string Name
    {
        get => "mega";
        set => throw new NotSupportedException("Setting the 'Names' property is not supported.");
    }

    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is not IfMegaVar megaVar)
        {
            return false;
        }
        var format = formattingInfo.Format;
        var options = format?.Split('|');
        if (options == null)
        {
            throw new LocException(
                $"Format expression must contain at least 1 option. format={formattingInfo.Format}.");
        }
        if (options.Count > 2)
        {
            throw new LocException(
                $"Format expression cannot contain more than 2 options. num_of_options={options.Count} format={formattingInfo.Format}.");
        }
        var megaText = options[0];
        var normalText = options.Count > 1 ? options[1] : null;
        switch (megaVar.MegaDisplay)
        {
            case MegaDisplay.Normal:
                if (normalText != null)
                {
                    formattingInfo.FormatAsChild(normalText, formattingInfo.CurrentValue);
                }
                break;
            case MegaDisplay.Mega:
                formattingInfo.FormatAsChild(megaText, formattingInfo.CurrentValue);
                break;
            case MegaDisplay.MegaPreview:
                formattingInfo.Write("[green]");
                formattingInfo.FormatAsChild(megaText, formattingInfo.CurrentValue);
                formattingInfo.Write("[/green]");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(megaVar.MegaDisplay),
                    megaVar.MegaDisplay, "Unexpected MegaDisplay value.");
        }
        return true;
    }
}
