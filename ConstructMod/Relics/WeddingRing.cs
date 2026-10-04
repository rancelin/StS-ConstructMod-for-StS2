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
/// Card matching goes through <see cref="CardModel.DeckVersion"/>: combat piles hold CLONES
/// of the run-deck cards (Player.PopulateCombatState clones each deck card into the draw
/// pile with a DeckVersion back-pointer), so direct reference comparison against the chosen
/// deck cards never matches — the clone's DeckVersion is the link.
///
/// NOTE (save/load limitation): the two chosen cards are held as in-memory references.
/// Persisting the selection (card id + upgrade state) is deferred until the save-support pass.
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
        // FromDeckGeneric is the vanilla pickup-time deck-selection API (same family as
        // Astrolabe's FromDeckForTransformation) — it takes the Player directly and shows the
        // deck-selection grid even outside the standard reward flow (e.g. console-granted).
        var prefs = new CardSelectorPrefs(
            new LocString("cards", "CONSTRUCTMOD-WEDDINGRING.selectionScreenPrompt"), 2, 2);
        var chosen = (await CardSelectCmd.FromDeckGeneric(Owner, prefs,
            c => !c.Keywords.Contains(CardKeyword.Unplayable))).ToList();
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

        // Match via the played clone's DeckVersion back-pointer to the chosen deck cards.
        // (Cards generated mid-combat have a null DeckVersion and never match.)
        var playedDeckVersion = cardPlay.Card.DeckVersion;
        CardModel? partnerDeckCard = ReferenceEquals(playedDeckVersion, _card1) ? _card2
            : ReferenceEquals(playedDeckVersion, _card2) ? _card1
            : null;
        if (partnerDeckCard == null) return;

        // Once per turn.
        _triggeredThisTurn = true;
        Flash();

        // Find the partner's combat clone in hand, draw, or discard (StS1 search order).
        CardModel? partner = null;
        foreach (var pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            partner = pileType.GetPile(Owner).Cards.FirstOrDefault(c =>
                !ReferenceEquals(c, cardPlay.Card) && ReferenceEquals(c.DeckVersion, partnerDeckCard));
            if (partner != null) break;
        }
        if (partner == null) return;

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
