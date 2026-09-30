using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class RetainRandomPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Retain Random",
        Description: "At the end of your turn, Retain {Amount} random card(s).",
        SmartDescription: "At the end of your turn, Retain {Amount} random card(s).");

    public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;
        if (PileType.Hand.GetPile(player) is not { } hand) return Task.CompletedTask;
        var candidates = hand.Cards
            .Where(c => !c.ShouldRetainThisTurn)
            .Where(c => c.Type is not (CardType.Status or CardType.Curse) || c.EnergyCost.Canonical >= 0)
            .ToList();
        var rng = Owner.Player!.RunState.Rng.CombatCardSelection;
        for (var i = 0; i < Amount && candidates.Count > 0; i++)
        {
            var card = candidates[rng.NextInt(candidates.Count)];
            card.GiveSingleTurnRetain();
            candidates.Remove(card);
        }
        return Task.CompletedTask;
    }
}
