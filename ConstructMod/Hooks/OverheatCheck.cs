using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Combat;
using ConstructMod.Cards;
using ConstructMod.Powers;

namespace ConstructMod.Hooks;

/// <summary>
/// The overheat check: after every cycle, iterate all combat piles and transform any
/// <see cref="AbstractConstructCard"/> whose <see cref="AbstractConstructCard.Overheat"/>
/// threshold has been reached (CycleCount &gt;= Overheat) into a <see cref="Burn"/>.
/// Skipped entirely while <see cref="FlashFreezePower"/> is active. Fires
/// <see cref="IAfterCardOverheated"/> for each transformed card.
/// </summary>
public static class OverheatCheck
{
    public static async Task Run(PlayerChoiceContext ctx, Player player)
    {
        if (player == null) return;
        if (player.Creature.HasPower<FlashFreezePower>()) return;
        var cycles = CycleCount.GetCyclesThisTurn(player);
        if (cycles <= 0) return;

        // Iterate draw, hand, discard. (Limbo / in-use cards are mid-animation; transforming
        // them is risky in StS2 and the original only did so for the cardInUse special case.
        // For the port we skip those — overheat fires on cards sitting in the main piles.)
        var overheating = new List<CardModel>();
        foreach (var pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            foreach (var card in pileType.GetPile(player).Cards.ToList())
            {
                if (card is AbstractConstructCard acc && acc.Overheat > 0 && cycles >= acc.Overheat)
                {
                    overheating.Add(card);
                }
            }
        }

        var combatState = player.Creature.CombatState;
        foreach (var card in overheating)
        {
            // TransformTo<Burn> handles the in-pile replacement + VFX.
            await CardCmd.TransformTo<Burn>(card);
            // Notify powers (e.g. Agitation) that this card overheated. The original card ref is
            // passed (it has been removed from its pile by TransformTo, but the object still exists).
            await CycleHook.AfterCardOverheated(combatState, ctx, card);
        }
    }
}
