using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ConstructMod.Cards;

/// <summary>
/// The display state for the {IfMega:...} description macro — mirrors vanilla's UpgradeDisplay.
/// </summary>
public enum MegaDisplay
{
    /// <summary>Not mega-upgraded — the normal branch renders.</summary>
    Normal,

    /// <summary>Mega-upgraded — the mega branch renders.</summary>
    Mega,

    /// <summary>Previewing the upgrade that would reach mega — the mega branch renders in green.</summary>
    MegaPreview,
}

/// <summary>
/// A <see cref="DynamicVar"/> carrying the mega-upgrade display state for card descriptions,
/// mirroring vanilla's IfUpgradedVar. Referenced in loc as {IfMega:mega:megaText|normalText} and
/// interpreted by <see cref="ShowIfMegaFormatter"/>. Added to the description by
/// AbstractConstructCard.AddExtraArgsToDescription. Usage: {IfMega:mega:megaText|normalText}.
/// </summary>
public class IfMegaVar : DynamicVar
{
    public const string DefaultName = "IfMega";

    public MegaDisplay megaDisplay;

    public IfMegaVar(MegaDisplay display) : base(DefaultName, (int)display)
    {
        megaDisplay = display;
    }
}
