using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class OverclockPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Overclock",
        Description: "#At the start of your turn, draw {Amount} cards and add a *Burn* to your hand.",
        SmartDescription: "#At the start of your turn, draw {Amount} cards and add a *Burn* to your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState is not { } combat) return;
        Flash();
        // Draw {Amount} cards (Amount is the draw count from the card's "Cards" var), but add exactly
        // 1 Burn per turn (matching the original StS1 OverclockPower: draw = statMultiplier, Burn = 1).
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
        var burn = combat.CreateCard(ModelDb.Card<Burn>(), Owner.Player);
        await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, Owner.Player);
    }
}
