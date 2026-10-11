using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

/// <summary>
/// Stasis (StS1 Uncommon): bank a hand card into Self-Stasis; the next Stasis play
/// returns the banked card force-mega-upgraded N times, then banks a new card.
/// 2-tier ladder: +1 = 1→2 copies, +2 = mega (2→3 copies, Innate + Retain).
/// Micro-deviation: the banked snapshot is a clone (upgrades preserved) rather than
/// StS1's fresh base copy — the force-mega reclaim saturates to the same end state
/// for Construct cards either way.
/// </summary>
public class Stasis : AbstractConstructCard
{
    // 2-tier ladder: +1 = +1 copy, +2 = mega (+1 copy, Innate, Retain).
    public override int IntrinsicMaxUpgradeLevel => 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            // Mega tier only (DarkFlames/PowerUp gating pattern).
            if (IsMegaUpgraded)
            {
                yield return CardKeyword.Innate;
                yield return CardKeyword.Retain;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Copies", 1m)];

    public Stasis() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Stasis",
        "#Exhaust a card.\nNext time you play this, add {Copies} *Mega-upgraded* copy/copies of that card to your hand.{IfMega:mega:\n*Innate*. *Retain*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var copies = DynamicVars["Copies"].IntValue;
        // 1. Reclaim the previously banked card (if any): force-mega it, return N copies.
        var old = Owner.Creature.Powers.OfType<ConstructStasisPower>().FirstOrDefault();
        if (old?.HeldCard != null)
        {
            var count = (int)old.Amount;
            var reclaimed = old.HeldCard.CreateClone();
            if (reclaimed is AbstractConstructCard construct)
                ForceUpgradeToMax(reclaimed);
            else if (reclaimed.IsUpgradable)
                CardCmd.Upgrade(reclaimed);
            for (var i = 0; i < count; i++)
            {
                var copy = reclaimed.CreateClone();
                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            }
            await PowerCmd.Remove(old);
        }
        // 2. Bank a new card from hand (StS1 banks anything — no filter). This Stasis
        // itself is in the Play pile, so it can't self-select.
        var candidates = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (candidates.Count == 0) return;
        CardModel? chosen = candidates.Count == 1
            ? candidates[0]
            : (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(
                    new LocString("cards", "CONSTRUCTMOD-STASIS.selectionScreenPrompt"), 1),
                context: choiceContext, player: Owner,
                filter: c => true, source: this)).FirstOrDefault();
        if (chosen == null) return;
        var banked = chosen.CreateClone();
        await PowerCmd.Apply<ConstructStasisPower>(choiceContext, Owner.Creature,
            copies, Owner.Creature, this);
        Owner.Creature.Powers.OfType<ConstructStasisPower>().FirstOrDefault()?.SetHeldCard(banked);
        await CardCmd.Exhaust(choiceContext, chosen);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: +1 copy (1 → 2).
            DynamicVars["Copies"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            // Mega: +1 copy (2 → 3), Innate, Retain.
            DynamicVars["Copies"].UpgradeValueBy(1m);
            AddKeyword(CardKeyword.Innate);
            AddKeyword(CardKeyword.Retain);
        }
    }
}
