using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// At the end of your turn, gain <see cref="PowerModel.Amount"/> Block (not boosted by Strength).
/// Matches the StS1 Metallicize behavior and Downfall's MetallicizePower. This is distinct from
/// vanilla StS2's <c>PlatingPower</c> in name only (vanilla Plating == end-of-turn block, no
/// lose-on-hit, same as Metallicize) — this power exists so the card text can say "Metallicize"
/// with its own icon/loc rather than reusing the vanilla "Plating" identity.
/// </summary>
public class MetallicizePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Metallicize",
        Description: "#At the end of your turn, gain {Amount} *Block*.",
        SmartDescription: "#At the end of your turn, gain {Amount} *Block*.");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, (int)Amount, BlockProps.nonCardUnpowered, null);
    }
}
