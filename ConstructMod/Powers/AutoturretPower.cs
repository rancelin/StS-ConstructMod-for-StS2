using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Powers;

public class AutoturretPower : CustomPowerModel, IAfterCardCycled
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Autoturret",
        Description: "Whenever a card Cycles, deal {Amount} damage to a random enemy.",
        SmartDescription: "Whenever a card Cycles, deal damage to a random enemy.");

    public async Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        if (Owner.CombatState == null) return;
        Flash();
        var enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return;
        await CreatureCmd.Damage(ctx, enemy, Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner);
    }
}
