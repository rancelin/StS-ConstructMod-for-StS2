using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Powers;

public class PanicFirePower : CustomPowerModel, IAfterCardCycled
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Panic Fire",
        Description: "Whenever a non-Upgraded card Cycles, Exhaust it and deal {Amount} damage to a random enemy.",
        SmartDescription: "Whenever a non-Upgraded card Cycles, Exhaust it and deal {Amount} damage to a random enemy.");

    public async Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card)
    {
        if (CombatState is not { } combat) return;
        if (card.IsUpgraded) return;
        Flash();
        await CardPileCmd.RemoveFromCombat(card);
        var enemy = Owner.Player?.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies);
        if (enemy != null)
        {
            await CreatureCmd.Damage(ctx, enemy, Amount, ValueProp.Unpowered, Owner);
        }
    }
}
