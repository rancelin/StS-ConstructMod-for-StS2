using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// Upon pickup, choose 2 cards in your deck. Once per turn, after you play one of them,
/// play the other too (for free, from your hand, draw pile, or discard pile).
///
/// NOTE (save/load limitation): the two chosen cards are held as in-memory references.
/// BaseLib's SavedSpireField doesn't support storing card references, so after loading a
/// save the selection is lost (the relic does nothing until re-obtained). Persisting the
/// selection (card id + upgrade state) is deferred until the save-support pass.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class WeddingRing : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient; // StS1 BOSS tier → StS2 Ancient (boss-reward tier)

    public override bool HasUponPickupEffect => true;

    private CardModel? _card1;
    private CardModel? _card2;
    private bool _triggeredThisTurn;

    public override async Task AfterObtained()
    {
        var deck = PileType.Deck.GetPile(Owner).Cards
            // StS1 filter: cost > -2 (excludes unplayable cards; X-cost is allowed).
            .Where(c => !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();
        if (deck.Count == 0) return;

        var prefs = new CardSelectorPrefs(
            new LocString("cards", "CONSTRUCTMOD-WEDDINGRING.selectionScreenPrompt"), 2, 2);
        var chosen = (await CardSelectCmd.FromSimpleGrid(
            new ThrowingPlayerChoiceContext(), deck, Owner, prefs)).ToList();
        if (chosen.Count < 2) return;
        _card1 = chosen[0];
        _card2 = chosen[1];
        Flash();
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner) _triggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner) return;
        if (_card1 == null || _card2 == null) return;
        if (_triggeredThisTurn) return;
        var played = cardPlay.Card;
        if (!ReferenceEquals(played, _card1) && !ReferenceEquals(played, _card2)) return;

        // Once per turn.
        _triggeredThisTurn = true;
        Flash();

        // Find the partner (the OTHER married card) in hand, draw, or discard.
        CardModel? partner = null;
        foreach (var pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            partner = pileType.GetPile(Owner).Cards.FirstOrDefault(c =>
                ReferenceEquals(c, _card1) || ReferenceEquals(c, _card2));
            if (partner != null) break;
        }
        if (partner == null || ReferenceEquals(partner, played)) return;

        // Target: the played card's target, or a random enemy.
        var target = cardPlay.Target;
        if (target == null || target.IsDead)
        {
            target = Owner.Creature.CombatState is { } combat
                ? Owner.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies)
                : null;
        }

        // AutoPlay is free by definition; it also moves the card out of draw/discard on its own.
        await CardCmd.AutoPlay(choiceContext, partner, target);
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Wedding Ring",
        Description: "#Upon pickup, choose 2 cards. Once per turn, after you play one of them, play the other too.",
        Flavor: "Till death do them part.");
}
