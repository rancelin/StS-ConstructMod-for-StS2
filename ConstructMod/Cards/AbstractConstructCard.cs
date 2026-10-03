using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Cards;

/// <summary>
/// Base class for all Construct cards. Uses in-code localization via BaseLib's CardLoc.
/// </summary>
[Pool(typeof(ConstructCardPool))]
public abstract class AbstractConstructCard : CustomCardModel
{
    // Per-card overheat threshold. -1 (default) = card does not participate in the Heat system.
    // A positive value N means the card turns into a Burn when CycleCount reaches N in a single turn.
    // Modified by UpgradeOverheat() and Coolant. Preserved on CreateClone() via CopyOnClone.
    private static readonly SpireField<CardModel, int> OverheatField =
        new SpireField<CardModel, int>(_ => -1).CopyOnClone();

    // Display flag: true when overheat was changed by an upgrade (so the !O! value renders green).
    private static readonly SpireField<CardModel, bool> UpgradedOverheatField =
        new SpireField<CardModel, bool>(_ => false).CopyOnClone();

    protected AbstractConstructCard(int baseCost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true, bool autoAdd = true)
        : base(baseCost, type, rarity, target, showInCardLibrary, autoAdd)
    {
    }

    /// <summary>The per-card overheat threshold. -1 = no overheat.</summary>
    public int Overheat
    {
        get => OverheatField.Get(this);
        set => OverheatField.Set(this, value);
    }

    /// <summary>Whether the overheat value was changed by an upgrade (for green-number display).</summary>
    public bool UpgradedOverheat
    {
        get => UpgradedOverheatField.Get(this);
        set => UpgradedOverheatField.Set(this, value);
    }

    /// <summary>Raise the overheat threshold by <paramref name="amount"/> (e.g. +5 from Coolant).</summary>
    public void UpgradeOverheat(int amount)
    {
        Overheat += amount;
        UpgradedOverheat = true;
    }
}
