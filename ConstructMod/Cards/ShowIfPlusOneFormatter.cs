using System;
using MegaCrit.Sts2.Core.Localization;
using SmartFormat.Core.Extensions;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

/// <summary>
/// A SmartFormat formatter for {IfPlusOne:plusone:plusOneText|} — the multi-level-aware stand-in for
/// vanilla's ShowIfUpgradedFormatter / {IfUpgraded:show:...}. Named "plusone" (SmartFormat rejects
/// duplicate formatter names, so it cannot share the vanilla "show" name). Registered automatically
/// via BaseLib's <see cref="IAutoRegisterFormatSpecifier"/>.
/// </summary>
public class ShowIfPlusOneFormatter : IFormatter, IAutoRegisterFormatSpecifier
{
    public string Name
    {
        get => "plusone";
        set => throw new NotSupportedException("Setting the 'Name' property is not supported.");
    }

    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is not IfPlusOneVar plusOneVar)
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
        var plusOneText = options[0];
        var hiddenText = options.Count > 1 ? options[1] : null;
        switch (plusOneVar.PlusOneDisplay)
        {
            case PlusOneDisplay.Hidden:
                if (hiddenText != null)
                {
                    formattingInfo.FormatAsChild(hiddenText, formattingInfo.CurrentValue);
                }
                break;
            case PlusOneDisplay.Shown:
                formattingInfo.FormatAsChild(plusOneText, formattingInfo.CurrentValue);
                break;
            case PlusOneDisplay.ShownGreen:
                formattingInfo.Write("[green]");
                formattingInfo.FormatAsChild(plusOneText, formattingInfo.CurrentValue);
                formattingInfo.Write("[/green]");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(plusOneVar.PlusOneDisplay),
                    plusOneVar.PlusOneDisplay, "Unexpected PlusOneDisplay value.");
        }
        return true;
    }
}
