using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Cards;
using ConstructMod.Pools;

namespace ConstructMod.Potions;

/// <summary>
/// Mega-upgrade a random eligible card in your hand (the original's MegaPotion: prefer a
/// Construct card that hasn't reached its mega tier and saturate it; fall back to a single
/// regular upgrade of any upgradable card so the potion is never dead).
/// </summary>
[Pool(typeof(ConstructPotionPool))]
public class MegaPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (Owner == null) return Task.CompletedTask;
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();

        // Prefer a Construct card with room to grow (not yet at its intrinsic/mega ceiling).
        var megaCandidates = hand.OfType<AbstractConstructCard>()
            .Where(c => c.CurrentUpgradeLevel < c.IntrinsicMaxUpgradeLevel)
            .ToList();
        if (megaCandidates.Count > 0)
        {
            var card = megaCandidates[Owner.RunState.Rng.CombatCardSelection.NextInt(0, megaCandidates.Count)];
            AbstractConstructCard.ForceUpgradeToMax(card);
            return Task.CompletedTask;
        }

        // Fallback: a single regular upgrade of any upgradable card.
        var fallback = hand.Where(c => c.IsUpgradable).ToList();
        if (fallback.Count > 0)
        {
            var card = fallback[Owner.RunState.Rng.CombatCardSelection.NextInt(0, fallback.Count)];
            CardCmd.Upgrade(card);
        }
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new PotionLoc(
        Title: "Mega Potion",
        Description: "#*Mega-upgrade* a random eligible card in your hand.");
}
