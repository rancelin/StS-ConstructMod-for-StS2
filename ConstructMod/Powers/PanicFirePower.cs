using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class PanicFirePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Panic Fire",
        Description: "Whenever a non-Upgraded card Cycles, Exhaust it and deal {Amount} damage to a random enemy.",
        SmartDescription: "Whenever a non-Upgraded card Cycles, Exhaust it and deal {Amount} damage to a random enemy.");

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        CycleEvents.CardCycled += OnCardCycled;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        CycleEvents.CardCycled -= OnCardCycled;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        CycleEvents.CardCycled -= OnCardCycled;
        return Task.CompletedTask;
    }

    private async Task OnCardCycled(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (CombatState is not { } combat) return;
        if (card.IsUpgraded) return;
        Flash();
        await CardPileCmd.RemoveFromCombat(card);
        var enemy = Owner.Player?.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies);
        if (enemy != null)
        {
            await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner);
        }
    }
}
