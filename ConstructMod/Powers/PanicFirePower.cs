using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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
        Description: "#Whenever a non-Upgraded card Cycles, Exhaust it and deal {Amount} damage to a random enemy.",
        SmartDescription: "#Whenever a non-Upgraded card Cycles, Exhaust it and deal {Amount} damage to a random enemy.");

    public async Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card)
    {
        // Damage only — the exhaust itself is handled by AbstractCycleCard, which
        // routes the cycling card straight to the exhaust pile when this power is
        // up (mirrors StS1, where CycleCardAction takes the exhaust branch up front
        // and the power's onCycleCard only queues damage). Listeners must NOT move
        // piles here: anything done to the card is undone by the cycle's
        // discard/draw step that runs after this hook.
        if (CombatState is not { } combat) return;
        if (card.IsUpgraded) return;
        if (CombatManager.Instance.IsOverOrEnding) return;
        Flash();
        var enemy = Owner.Player?.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies);
        if (enemy != null)
        {
            await CreatureCmd.Damage(ctx, enemy, Amount, ValueProp.Unpowered, Owner);
        }
    }
}
