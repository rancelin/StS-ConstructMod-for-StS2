using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Cards;

/// <summary>
/// A <see cref="DynamicVar"/> exposing the <see cref="AbstractConstructCard.Overheat"/>
/// threshold for card text. Renders as {Overheat} in descriptions. Reads the live value
/// (including mid-combat Coolant modifications) via <see cref="GetBaseValueForIConvertible"/>,
/// and updates the preview to match so the card always shows the current threshold.
/// </summary>
public class OverheatVar : DynamicVar
{
    public const string Key = "Overheat";

    public OverheatVar() : base(Key, -1m)
    {
    }

    public override void SetOwner(AbstractModel owner)
    {
        base.SetOwner(owner);
        // Sync BaseValue from the card's Overheat field so the var starts correct.
        if (owner is AbstractConstructCard acc)
        {
            BaseValue = acc.Overheat;
        }
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        // Read the live Overheat value (may have been modified by Coolant mid-combat) so the
        // card description always reflects the current threshold.
        if (card is AbstractConstructCard acc)
        {
            PreviewValue = acc.Overheat;
        }
    }

    protected override decimal GetBaseValueForIConvertible()
    {
        // Read the live value from the owning card so {Overheat} resolves to the current
        // threshold even after Coolant modifies it.
        if (_owner is AbstractConstructCard acc)
        {
            return acc.Overheat;
        }
        return BaseValue;
    }

    public override string ToString() =>
        ((int)GetBaseValueForIConvertible()).ToString(CultureInfo.InvariantCulture);
}
