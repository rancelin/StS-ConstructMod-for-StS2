using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Cards;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// Purple Ember (StS1 Rare): entering a Rest Site arms it (pulse); at the start of the
/// next combat the starting hand is force-mega-upgraded, then it disarms. Follows the
/// StS1 ACTION (permanent upgrades), not its stale "until the end of combat" text.
/// Armed state persists via SavedSpireField (rest → save → combat spans sessions).
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class PurpleEmber : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    private static readonly SavedSpireField<PurpleEmber, bool> Armed = new(() => false, "ConstructMod_PurpleEmber_Armed");

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is RestSiteRoom)
        {
            Armed.Set(this, true);
            Flash();
            Status = RelicStatus.Active; // pulse while armed
        }
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        if (!Armed.Get(this)) return Task.CompletedTask;
        Armed.Set(this, false);
        Status = RelicStatus.Normal;
        Flash();
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (var card in hand)
        {
            if (card.IsUpgradable || (card is AbstractConstructCard construct
                && construct.CurrentUpgradeLevel < construct.IntrinsicMaxUpgradeLevel))
            {
                AbstractConstructCard.ForceUpgradeToMax(card);
            }
        }
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Purple Ember",
        Description: "#When you enter a Rest Site, your starting hand next combat is #yMega-upgraded.",
        Flavor: "A ghostly, flickering flame. It seems to absorb the power of nearby fires.");
}
