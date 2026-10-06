using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Cards;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// Upon pickup, Mega-upgrade a random Construct card in your deck (force it to its full upgrade
/// ladder — the original's onEquip with the forced-upgrade saturation loop). Cards already at
/// their mega tier are skipped; if none are eligible, the relic does nothing.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class MegaBattery : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(ConstructKeywords.MegaUpgrade)];

    public override Task AfterObtained()
    {
        // Eligible: any Construct card that hasn't reached its intrinsic (mega) ceiling.
        // In Phase 1 only cards with an implemented mega ladder have IntrinsicMaxUpgradeLevel > 1.
        var candidates = PileType.Deck.GetPile(Owner).Cards
            .OfType<AbstractConstructCard>()
            .Where(c => c.CurrentUpgradeLevel < c.IntrinsicMaxUpgradeLevel)
            .ToList();
        if (candidates.Count == 0) return Task.CompletedTask;
        Flash();
        var card = candidates[Owner.RunState.Rng.CombatCardSelection.NextInt(0, candidates.Count)];
        // Saturate to mega via the vanilla upgrade driver (history + upgrade VFX per level).
        AbstractConstructCard.ForceUpgradeToMax(card);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Mega Battery",
        Description: "#Upon pickup, *Mega-upgrade* a random eligible Construct card in your deck.",
        Flavor: "Handles like a brick, hits like a train.");
}
