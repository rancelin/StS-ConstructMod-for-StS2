using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class ElectricArmorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Electric Armor",
        Description: "This turn, when an enemy attacks you, it takes damage equal to your Dexterity. Lasts {Amount} turn(s).",
        SmartDescription: "When an enemy attacks you, it takes damage equal to your Dexterity.");

    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // Log at the very top so we can see whether the hook fires at all, and why we might bail early.
        ConstructModMain.Logger.Info($"ElectricArmorPower.BeforeDamageReceived: target={target == Owner}, dealer={(dealer == null ? "null" : "set")}, props={props}, IsPoweredAttack={props.IsPoweredAttack()}, amount={amount}.");
        if (target != Owner || dealer == null || !props.IsPoweredAttack()) return;
        var dexterity = Owner.GetPower<DexterityPower>()?.Amount ?? 0;
        ConstructModMain.Logger.Info($"ElectricArmorPower.BeforeDamageReceived: Owner Dexterity = {dexterity}.");
        if (dexterity <= 0) return;
        ConstructModMain.Logger.Info($"ElectricArmorPower.BeforeDamageReceived: reflecting {dexterity} Dexterity damage back at the attacker.");
        Flash();
        // Pass Owner as the dealer (matching vanilla ThornsPower) — a null dealer can cause the
        // damage to be dropped (the game suppresses unattributed damage to avoid thorns loops).
        await CreatureCmd.Damage(choiceContext, dealer, (int)dexterity,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side, System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (Amount > 1)
        {
            await PowerCmd.TickDownDuration(this);
            return;
        }
        await PowerCmd.Remove(this);
    }
}
