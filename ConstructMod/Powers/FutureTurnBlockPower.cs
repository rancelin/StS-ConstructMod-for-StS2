using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// Grants <see cref="PowerModel.Amount"/> Block two turns from now: on the next
/// <see cref="AfterBlockCleared"/> (start of next turn), applies a vanilla
/// <see cref="BlockNextTurnPower"/> for the same amount, then removes itself — so the
/// block arrives one turn later. Matches the StS1 FutureTurnBlockPower used by
/// FlammableFog's mega-upgrade. Ported as the upgraded FlammableFog effect
/// (mega folded into the normal upgrade).
/// </summary>
public class FutureTurnBlockPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Future Block",
        Description: "#Two turns from now, gain {Amount} *Block*.",
        SmartDescription: "#Two turns from now, gain {Amount} *Block*.");

    public override async Task AfterBlockCleared(Creature creature)
    {
        if (creature != Owner) return;
        Flash();
        await PowerCmd.Apply<BlockNextTurnPower>(new ThrowingPlayerChoiceContext(),
            Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}
