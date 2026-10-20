using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class SyphonPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Syphon",
        Description: "#Whenever this enemy takes attack damage, its attacker draws 1 card.",
        SmartDescription: "#Whenever {OwnerName} takes attack damage, its attacker draws 1 card.");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (result.TotalDamage <= 0) return;
        if (!props.HasFlag(ValueProp.Move)) return;
        var player = dealer?.Player;
        if (player == null) return;
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, player, cardSource!);
    }

    // StS1: atEndOfRound() reduces the amount by 1 (and removes it at 0) — a turn-based debuff.
    // Vanilla debuffs (Vulnerable/Weak) tick at the end of the Enemy side turn; mirror that so the
    // power actually expires instead of lasting the whole combat.
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy) await PowerCmd.TickDownDuration(this);
    }
}
