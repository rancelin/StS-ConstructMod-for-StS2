using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class ZapperPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Zapper",
        Description: "Whenever you gain Strength or Dexterity, deal {Amount} damage to a random enemy.",
        SmartDescription: "Whenever you gain Strength or Dexterity, deal {Amount} damage to a random enemy.");

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this) return;
        if (power.Owner != Owner) return;
        if (amount <= 0) return;
        if (power is not (StrengthPower or DexterityPower)) return;
        if (CombatState is not { } combat) return;
        if (!combat.HittableEnemies.Any()) return;
        Flash();
        var enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies)!;
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemy, Amount, ValueProp.Unpowered, Owner);
    }
}
