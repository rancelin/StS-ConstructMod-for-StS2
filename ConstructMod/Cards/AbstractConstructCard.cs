using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Cards;

/// <summary>
/// Base class for all Construct cards. Uses in-code localization via BaseLib's CardLoc.
///
/// Mega-upgrade support (the original mod's second upgrade tier) is built on StS2's native
/// multi-level upgrade system: <see cref="CardModel.MaxUpgradeLevel"/> (default 1) with
/// <see cref="CardModel.CurrentUpgradeLevel"/>. A card opts into the mega tier by overriding
/// <see cref="IntrinsicMaxUpgradeLevel"/> to 2 and branching its OnUpgrade on
/// <see cref="CardModel.CurrentUpgradeLevel"/> (1 = regular delta, 2 = mega delta). The vanilla
/// engine then handles eligibility (<c>IsUpgradable</c>), the "Name+1"/"Name+2" title rendering,
/// the save/load per-level replay, and the upgrade flows with zero patches — the same foundation
/// Downfall (RefractedBeam) and TheTailor (99999-level cards) build on.
///
/// Gating (matching the original's ClockworkPhoenix rules): the player may only CHOOSE a mega
/// upgrade at the smith while owning <see cref="Relics.ClockworkPhoenix"/> (and never in combat);
/// forced mega sources (MegaBattery, MegaPotion, ...) bypass the gate via
/// <see cref="ForceUpgradeToMax"/>, the StS2 equivalent of the original's forcedUpgrade flag.
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

    // Transient "forced upgrade in progress" flag (the original's forcedUpgrade). While set,
    // MaxUpgradeLevel allows one more level regardless of the Phoenix gate, so programmatic
    // mega sources can saturate the card. Never persisted.
    private static readonly ConditionalWeakTable<CardModel, StrongBox<bool>> ForcingUpgrades = new();

    protected AbstractConstructCard(int baseCost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true, bool autoAdd = true)
        : base(baseCost, type, rarity, target, showInCardLibrary, autoAdd)
    {
    }

    /// <summary>
    /// The card's true upgrade ceiling: 1 = single-tier (no mega), 2 = regular + mega tier.
    /// Override to 2 when the card's OnUpgrade implements its per-level mega ladder
    /// (branching on <see cref="CardModel.CurrentUpgradeLevel"/>).
    /// </summary>
    public virtual int IntrinsicMaxUpgradeLevel => 1;

    /// <summary>
    /// The engine-facing upgrade cap. Applies the mega gating on top of
    /// <see cref="IntrinsicMaxUpgradeLevel"/>:
    /// <list type="bullet">
    /// <item>Forced upgrade in progress → one more level than current (bounded by intrinsic).</item>
    /// <item>No owner yet (save/load replay, canonical/library instances) → intrinsic, so saved
    /// mega cards always reconstruct regardless of Phoenix.</item>
    /// <item>In combat → regular tier only (the original never allowed player-chosen mega
    /// mid-combat, even with Phoenix).</item>
    /// <item>Out of combat with <see cref="Relics.ClockworkPhoenix"/> → intrinsic.</item>
    /// <item>Otherwise → regular tier only.</item>
    /// </list>
    /// </summary>
    public override int MaxUpgradeLevel
    {
        get
        {
            if (ForcingUpgrades.TryGetValue(this, out var box) && box.Value)
            {
                return Math.Min(IntrinsicMaxUpgradeLevel, CurrentUpgradeLevel + 1);
            }
            if (Owner == null)
            {
                return IntrinsicMaxUpgradeLevel;
            }
            if (Owner.Creature.CombatState != null)
            {
                return Math.Min(IntrinsicMaxUpgradeLevel, 1);
            }
            return HasClockworkPhoenix()
                ? IntrinsicMaxUpgradeLevel
                : Math.Min(IntrinsicMaxUpgradeLevel, 1);
        }
    }

    private bool HasClockworkPhoenix() => Owner.Relics?.OfType<Relics.ClockworkPhoenix>().Any() == true;

    /// <summary>True once the card has reached its mega tier.</summary>
    public bool IsMegaUpgraded => CurrentUpgradeLevel >= IntrinsicMaxUpgradeLevel
        && IntrinsicMaxUpgradeLevel > 1;

    /// <summary>
    /// Saturate the card's upgrade ladder (the original's "upgrade(true) x5" idiom): force-upgrade
    /// until the card reaches <see cref="IntrinsicMaxUpgradeLevel"/>. Uses CardCmd.Upgrade so each
    /// level gets the vanilla history/VFX treatment. Safe on already-maxed cards (no-op).
    /// </summary>
    public static void ForceUpgradeToMax(CardModel card)
    {
        var box = ForcingUpgrades.GetOrCreateValue(card);
        var previous = box.Value;
        box.Value = true;
        try
        {
            while (card.IsUpgradable)
            {
                CardCmd.Upgrade(card);
            }
        }
        finally
        {
            box.Value = previous;
        }
    }

    /// <summary>
    /// Injects the {IfMega:...} description variable so loc text can branch per tier:
    /// {IfMega:mega:mega-only sentence.|} — handled by ShowIfMegaFormatter (registered via
    /// BaseLib's IAutoRegisterFormatSpecifier; named "mega" since SmartFormat forbids
    /// duplicate formatter names and vanilla already owns "show").
    /// </summary>
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        var display = IsMegaUpgraded ? MegaDisplay.Mega : MegaDisplay.Normal;
        // Upgrade previews of a card that would reach its mega tier show the mega branch in green
        // (mirrors the vanilla IfUpgraded UpgradePreview handling).
        if (UpgradePreviewType.IsPreview()
            && CurrentUpgradeLevel + 1 >= IntrinsicMaxUpgradeLevel
            && IntrinsicMaxUpgradeLevel > 1)
        {
            display = MegaDisplay.MegaPreview;
        }
        description.Add(new IfMegaVar(display));
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
