using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// At the end of your turn, gain <see cref="PowerModel.Amount"/> Block. Receiving unblocked
/// attack damage reduces Plated Armor by 1. Matches StS1 Plated Armor and Downfall's
/// PlatedArmorPower. Distinct from Metallicize in that plates are lost when hit.
/// </summary>
public class PlatedArmorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Plated Armor",
        Description: "#At the end of your turn, gain *Block*. Receiving unblocked attack damage reduces [gold]Plated Armor[/gold] by 1.",
        SmartDescription: "#At the end of your turn, gain {Amount} *Block*. Receiving unblocked attack damage reduces [gold]Plated Armor[/gold] by 1.");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage == 0 || !props.IsPoweredAttack()) return;
        await PowerCmd.Decrement(this);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, (int)Amount, BlockProps.nonCardUnpowered, null);
    }
}
