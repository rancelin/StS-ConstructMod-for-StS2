using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;using MegaCrit.Sts2.Core.Entities.Powers;using MegaCrit.Sts2.Core.Entities.Players;using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class AutoturretPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Autoturret",
        Description: "Whenever a card Cycles, deal {Amount} damage to a random enemy.",
        SmartDescription: "Whenever a card Cycles, deal damage to a random enemy.");

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

    private async Task OnCardCycled(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        Flash();
        var enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return;
        await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner);
    }
}
