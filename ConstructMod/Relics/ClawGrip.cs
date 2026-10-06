using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// At the end of your turn, Retain a random eligible card in your hand. At the start of
/// your next turn, that card costs 1 less.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class ClawGrip : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient; // StS1 BOSS tier → StS2 Ancient (boss-reward tier)

    // The card retained last turn (combat-only reference; the cost discount is per-turn so
    // it never needs to survive a save/load).
    private CardModel? _retainedCard;

    public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;
        var candidates = PileType.Hand.GetPile(Owner).Cards
            // StS1 filter: (costForTurn > 0 || !retain) && (not Status/Curse || cost > -2).
            // The second clause's StS2 equivalent (from RetainRandomPower):
            .Where(c => c.Type is not (CardType.Status or CardType.Curse) || c.EnergyCost.Canonical >= 0)
            .Where(c => c.EnergyCost.GetResolved() > 0 || !c.ShouldRetainThisTurn)
            .ToList();
        if (candidates.Count == 0) return Task.CompletedTask;
        var card = candidates[Owner.RunState.Rng.CombatCardSelection.NextInt(0, candidates.Count)];
        Flash();
        card.GiveSingleTurnRetain();
        _retainedCard = card;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || _retainedCard == null) return Task.CompletedTask;
        var card = _retainedCard;
        _retainedCard = null;
        // The retained card costs 1 less this turn (relative reduction, floor at free).
        card.EnergyCost.AddThisTurnOrUntilPlayed(-1, reduceOnly: true);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Claw Grip",
        Description: "#At the end of your turn, Retain a random card in your hand. At the start of your next turn, it costs 1 less.",
        Flavor: "Once it grabs hold, it doesn't let go.");
}
