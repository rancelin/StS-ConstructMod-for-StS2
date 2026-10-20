using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ConstructMod.Cards;

/// <summary>
/// A <see cref="CalculatedDamageVar"/> that displays 0 whenever its un-modified calculated value is
/// 0 — instead of showing the Strength-only value. Used by Rollout: "damage equal to N × cards
/// Cycled this turn" should read 0 when nothing has Cycled, and the card's execution already skips
/// the attack entirely in that case (so the displayed value matches what actually happens).
/// </summary>
/// <remarks>
/// The base <see cref="CalculatedDamageVar.UpdateCardPreview"/> routes through the full damage
/// pipeline (which adds Strength), so a 0-base Rollout with 1 Strength would preview "1" while the
/// card deals 0. NOTE: this deliberately diverges from the StS1 original, whose applyPowers() would
/// let Strength add to the 0 base (dealing 1). The original description-vs-code intent still needs
/// an audit.
/// </remarks>
public class CycleDamageVar : CalculatedDamageVar
{
    public CycleDamageVar(ValueProp props) : base(props)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target,
        bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        if (!card.IsEnchantmentPreview && Calculate(target) <= 0m) PreviewValue = 0m;
    }
}
