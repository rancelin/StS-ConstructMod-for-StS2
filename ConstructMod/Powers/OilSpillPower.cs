using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// A debuff on an enemy (the StS1 "Oil"). The trigger is the PLAYER who applied it taking
/// damage from a Burn card (Burns deal their end-of-turn damage to the player — they never
/// hit the enemy), and the effect is the debuffed enemy (this power's owner) taking
/// <see cref="MegaCrit.Sts2.Core.Models.PowerModel.Amount"/> damage in return. Applied by
/// <see cref="Cards.OilSpill"/> (to all enemies when upgraded).
/// </summary>
public class OilSpillPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Oil",
        // Dumb/static description path gets NO variables injected (an unresolvable selector
        // breaks the whole format string). Static text only, like vanilla enemy debuffs;
        // the amount shows on the power's counter.
        Description: "#Whenever a *Burn* card hits the player, this character takes damage.",
        // Smart path (in-combat tooltip) DOES get {ApplierName} and {Amount} injected — both
        // derived from the power's own Applier (set by PowerCmd.Apply's applier argument).
        SmartDescription: "#Whenever a *Burn* card hits {ApplierName}, this character takes {Amount} damage.");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // Burns only damage the PLAYER (Burn.OnTurnEndInHand hits its owner). The power sits on
        // the enemy as a marker — so the trigger is the applier taking Burn damage, and the
        // effect is THIS power's owner (the oiled enemy) taking the damage.
        if (Applier == null || target != Applier) return;
        if (result.TotalDamage <= 0) return;
        if (cardSource is not Burn) return;
        Flash();
        // Unpowered damage (not boosted by Strength); dealer = the applier so kills are
        // credited to the player.
        await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unpowered, Applier);
    }
}
