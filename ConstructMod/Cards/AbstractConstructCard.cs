using MegaCrit.Sts2.Core.Entities.Cards;
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
    protected AbstractConstructCard(int baseCost, CardType type, CardRarity rarity, TargetType target)
        : base(baseCost, type, rarity, target)
    {
    }
}
