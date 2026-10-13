using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Supernova : AbstractConstructCard
{
    // 2-tier ladder: +1 = cost 3 → 2, +2 = mega (cost → 1, replacements upgraded).
    // (StS1 upgrades replacements only when mega — SupernovaAction(megaUpgraded) —
    // despite the regular text; code over text.)
    public override int IntrinsicMaxUpgradeLevel => 2;
    public Supernova() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Supernova",
        "#Exhaust ALL *Status* cards and replace them with random{IfMega:mega: *upgraded*|} cards.\nThose cards cost 0 this combat.");

    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } pcs || CombatState is not { } combat) return;
        // Replacement pool: all 5 vanilla character pools (StS1 returnTrulyRandomCardInCombat
        // equivalent), same allowlist as MemoryTap — playable rarities only, never Status
        // (a Status replacing a Status would churn forever) or Curse.
        var pools = new List<CardPoolModel>
        {
            ModelDb.CardPool<IroncladCardPool>(),
            ModelDb.CardPool<SilentCardPool>(),
            ModelDb.CardPool<DefectCardPool>(),
            ModelDb.CardPool<RegentCardPool>(),
            ModelDb.CardPool<NecrobinderCardPool>()
        };
        var options = pools
            .SelectMany(p => p.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Type != CardType.Curse && c.Type != CardType.Status)
            .Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .ToList();
        foreach (var pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            var statuses = pileType.GetPile(Owner).Cards.Where(c => c.Type == CardType.Status).ToList();
            foreach (var status in statuses)
            {
                await CardCmd.Exhaust(choiceContext, status);
                if (options.Count == 0) continue;
                var canonical = Owner.RunState.Rng.CombatCardGeneration.NextItem(options);
                if (canonical == null) continue;
                var replacement = combat.CreateCard(canonical, Owner);
                if (IsMegaUpgraded && replacement.IsUpgradable) CardCmd.Upgrade(replacement);
                replacement.EnergyCost.AddThisCombat(-9, reduceOnly: true);
                await CardPileCmd.AddGeneratedCardToCombat(replacement, pileType, Owner, CardPilePosition.Top);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: cost 3 → 2. Mega: cost → 1 (replacement upgrades move here too).
        EnergyCost.UpgradeBy(-1);
    }
}
