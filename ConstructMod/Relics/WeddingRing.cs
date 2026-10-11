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
/// NOTE (save/load): the two chosen cards are held as live run-deck references, backed
/// by persisted card-TYPE names (SavedSpireField). Type identity is upgrade-proof
/// (upgrade keeps the class) and duplicate-indifferent (two identical Strikes are
/// interchangeable marriage partners); transforms/removals correctly break the link.
/// Re-resolved lazily on first use after a load via EnsureResolved().
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class WeddingRing : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient; // StS1 BOSS tier → StS2 Ancient (boss-reward tier)

    public override bool HasUponPickupEffect => true;

    private static readonly SavedSpireField<WeddingRing, string> SavedCard1 =
        new(() => "", "ConstructMod_WeddingRing_Card1");
    private static readonly SavedSpireField<WeddingRing, string> SavedCard2 =
        new(() => "", "ConstructMod_WeddingRing_Card2");

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
        SavedCard1.Set(this, chosen[0].GetType().FullName ?? "");
        SavedCard2.Set(this, chosen[1].GetType().FullName ?? "");
        Flash();
    }

    /// <summary>
    /// Re-link the live deck references after a save/load (which re-instantiates every
    /// card, killing the in-memory references). Matches by card type: first deck card
    /// of the saved class. No-op when references are alive or nothing was ever chosen.
    /// </summary>
    private void EnsureResolved()
    {
        if (_card1 != null && _card2 != null) return;
        var deck = PileType.Deck.GetPile(Owner).Cards;
        if (_card1 == null)
        {
            var saved = SavedCard1.Get(this);
            if (!string.IsNullOrEmpty(saved))
                _card1 = deck.FirstOrDefault(c => c.GetType().FullName == saved);
        }
        if (_card2 == null)
        {
            var saved = SavedCard2.Get(this);
            if (!string.IsNullOrEmpty(saved))
                _card2 = deck.FirstOrDefault(c => c.GetType().FullName == saved);
        }
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner) _triggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner) return;
        EnsureResolved();
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
