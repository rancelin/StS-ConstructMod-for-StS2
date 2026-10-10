using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class OverclockPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Burns", 1m)];

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Overclock",
        Description: "#At the start of your turn, draw {Amount} cards and add {Burns} *Burn* to your hand.",
        SmartDescription: "#At the start of your turn, draw {Amount} cards and add {Burns} *Burn* to your hand.");

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        // StS1: draw = amount*mult, Burns = amount (stack count). The card bakes mult
        // into Amount (base Cards = 2, cost-only upgrade so it never changes), so
        // stacks = Amount/2. Sync the display var on every change (self-healing).
        if (power == this) SyncBurns();
        return Task.CompletedTask;
    }

    private void SyncBurns() => DynamicVars["Burns"].BaseValue = (int)(Amount / 2m);

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState is not { } combat) return;
        Flash();
        SyncBurns();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
        for (var i = 0; i < DynamicVars["Burns"].IntValue; i++)
        {
            var burn = combat.CreateCard(ModelDb.Card<Burn>(), Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, Owner.Player);
        }
    }
}
