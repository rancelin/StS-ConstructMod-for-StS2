using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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

/// <summary>
/// Faithful port of StS1 ElectricArmorPower: when attacked, reflect live Dexterity
/// as damage back. The Dexterity read happens at attack time (StS1 onAttacked), so
/// Dexterity gained after play still counts — and playing at 0 Dexterity (the card's
/// cycle condition) is a live trap, not a dead play. Replaces the earlier snapshot
/// (Downfall-style 1-turn Thorns), which granted nothing in exactly that common case.
/// </summary>
public class ElectricArmorPower : CustomPowerModel
{
    private bool _justApplied;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Electric Armor",
        Description: "#This turn, when an enemy attacks you, it takes damage equal to your *Dexterity*.",
        SmartDescription: "#This turn, when an enemy attacks you, it takes damage equal to your *Dexterity*.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        _justApplied = true;
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer == Owner) return;
        var dexterity = Owner.GetPower<DexterityPower>()?.Amount ?? 0;
        if (dexterity <= 0) return;
        Flash();
        // Unpowered thorns-equivalent (cf. Zapper/Synchronize reactive damage):
        // powered-only hooks don't chain off it, which covers StS1's THORNS/HP_LOSS
        // exclusion closely enough to avoid reflect loops.
        await CreatureCmd.Damage(choiceContext, dealer, dexterity, ValueProp.Unpowered, Owner);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return;
        // Skip the first turn-end after application so the armor survives the enemy turn.
        if (_justApplied)
        {
            _justApplied = false;
            return;
        }
        if (Amount > 1)
        {
            await PowerCmd.TickDownDuration(this);
            return;
        }
        await PowerCmd.Remove(this);
    }
}
