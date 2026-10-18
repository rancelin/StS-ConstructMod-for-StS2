using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ConstructMod.Cards;

/// <summary>
/// The display state for the {IfPlusOne:...} description macro: text that the REGULAR (+1) upgrade
/// introduces, which stays visible at +2.
/// </summary>
public enum PlusOneDisplay
{
    /// <summary>Not upgraded yet — the +1 text is not on the card, so nothing renders.</summary>
    Hidden,

    /// <summary>Already upgraded — the +1 text is on the card, rendered normally.</summary>
    Shown,

    /// <summary>Previewing the upgrade that reaches +1 — the +1 text renders in green.</summary>
    ShownGreen,
}

/// <summary>
/// A <see cref="DynamicVar"/> carrying the "text introduced by the regular (+1) upgrade" display
/// state for card descriptions. Referenced in loc as {IfPlusOne:plusone:plusOneText|} and interpreted
/// by <see cref="ShowIfPlusOneFormatter"/>. Added to the description by
/// AbstractConstructCard.AddExtraArgsToDescription.
///
/// <para>
/// Why this exists: vanilla's {IfUpgraded:show:...} only knows "upgraded or not", so on a two-tier
/// card it renders the +1 text green whenever the card is upgraded — including at the +1→+2 preview,
/// where that text is already printed on the card and is NOT part of what the mega upgrade adds.
/// This macro is the multi-level-aware equivalent of {IfUpgraded:show:...} and behaves identically
/// for single-tier cards.
/// </para>
/// </summary>
public class IfPlusOneVar : DynamicVar
{
    public const string DefaultName = "IfPlusOne";

    public PlusOneDisplay PlusOneDisplay;

    public IfPlusOneVar(PlusOneDisplay display) : base(DefaultName, (int)display)
    {
        PlusOneDisplay = display;
    }
}
