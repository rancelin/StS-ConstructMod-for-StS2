using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class MemoryTap : AbstractConstructCard
{
    // 2-tier ladder: +1 = cards arrive Upgraded, +2 = mega (twice the cards, cost → 3).
    public override int IntrinsicMaxUpgradeLevel => 2;
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    public MemoryTap() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Memory Tap",
        "#Add a random card from 3 different classes to your hand{IfMega:mega: twice|}. They cost 0 this turn.{IfPlusOne:plusone: Upgrade them.|}");

    private async Task AddOneClassCard(PlayerChoiceContext choiceContext, CardPoolModel pool)
    {
        if (CombatState is not { } combat) return;
        // Allowlist normal-playable rarities. This excludes Basic/Token/Status/Curse AND the
        // special reward rarities Ancient/Event/Quest (e.g. Suppress, an Ancient reward card,
        // which would otherwise be generatable and breaks balance).
        var options = pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type != CardType.Curse && c.Type != CardType.Status)
            .Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .ToList();
        if (options.Count == 0) return;
        var template = Owner.RunState.Rng.CombatCardGeneration.NextItem(options);
        if (template == null) return;
        var card = combat.CreateCard(template, Owner);
        if (IsUpgraded && card.IsUpgradable) CardCmd.Upgrade(card);
        card.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var pools = new List<CardPoolModel>
        {
            ModelDb.CardPool<IroncladCardPool>(),
            ModelDb.CardPool<SilentCardPool>(),
            ModelDb.CardPool<DefectCardPool>(),
            ModelDb.CardPool<RegentCardPool>(),
            ModelDb.CardPool<NecrobinderCardPool>()
        };
        var shuffled = pools.OrderBy(_ => Owner.RunState.Rng.CombatCardGeneration.NextFloat()).Take(3);
        // Mega tier: the whole set twice (6 cards).
        for (var round = 0; round < (IsMegaUpgraded ? 2 : 1); round++)
        {
            foreach (var pool in shuffled)
            {
                await AddOneClassCard(choiceContext, pool);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: no stat change (cards arrive Upgraded — text only). Mega: cost 2 → 3;
        // the doubled set moves here (OnPlay, gated on IsMegaUpgraded).
        if (CurrentUpgradeLevel == 2)
        {
            EnergyCost.UpgradeBy(1);
        }
    }
}
