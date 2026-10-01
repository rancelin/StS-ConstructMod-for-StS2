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
        "#Add a random card from 3 different classes to your hand. They cost 0 this turn.{IfUpgraded:show: Upgrade them.|}");

    private async Task AddOneClassCard(PlayerChoiceContext choiceContext, CardPoolModel pool)
    {
        if (CombatState is not { } combat) return;
        var options = pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type != CardType.Curse && c.Type != CardType.Status)
            .Where(c => c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Token)
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
        foreach (var pool in shuffled)
        {
            await AddOneClassCard(choiceContext, pool);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
