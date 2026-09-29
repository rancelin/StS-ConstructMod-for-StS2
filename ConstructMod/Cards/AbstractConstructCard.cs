using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

/// <summary>
/// Base class for all Construct cards. Uses in-code localization via BaseLib's CardLoc.
/// </summary>
public abstract class AbstractConstructCard : CustomCardModel
{
    protected AbstractConstructCard(int baseCost, CardType type, CardRarity rarity, TargetType target)
        : base(baseCost, type, rarity, target)
    {
    }
}
