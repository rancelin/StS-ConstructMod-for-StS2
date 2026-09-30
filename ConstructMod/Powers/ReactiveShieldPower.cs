using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class ReactiveShieldPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Reactive Shield",
        Description: "Whenever you gain Block, deal {Amount} damage to the lowest-HP enemy. It loses 1 Strength this turn.",
        SmartDescription: "Whenever you gain Block, deal {Amount} damage to the lowest-HP enemy. It loses 1 Strength this turn.");

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner || amount <= 0) return;
        if (CombatState is not { } combat) return;
        var enemies = combat.HittableEnemies;
        if (!enemies.Any()) return;
        Flash();
        var target = enemies.OrderBy(e => e.CurrentHp).First()!;
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, Amount, ValueProp.Unpowered, Owner);
        await PowerCmd.Apply<ReactiveShieldStrengthDownPower>(new ThrowingPlayerChoiceContext(), target, 1m, Owner, null);
    }
}
