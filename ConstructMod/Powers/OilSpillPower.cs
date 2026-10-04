using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class OilSpillPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Oil",
        // Dumb/static description path gets NO variables injected (no {Amount}/{OwnerName} —
        // an unresolvable selector breaks the whole format string, which is why the enemy
        // tooltip previously showed raw '{Owner}' '{Amount}'). Static text only, like
        // vanilla enemy debuffs; the amount shows on the power's counter.
        Description: "#Whenever a *Burn* card hits this character, it takes damage.",
        // Smart path (in-combat tooltip) DOES get {Amount} and {OwnerName} injected.
        SmartDescription: "#Whenever a *Burn* card hits {OwnerName}, it takes {Amount} damage.");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (result.TotalDamage <= 0) return;
        if (cardSource is not MegaCrit.Sts2.Core.Models.Cards.Burn) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unpowered, dealer!);
    }
}
