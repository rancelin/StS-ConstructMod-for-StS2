using System.Linq;
using MegaCrit.Sts2.Core.Extensions;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;using MegaCrit.Sts2.Core.Entities.Powers;using MegaCrit.Sts2.Core.Entities.Players;using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class EnhancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Enhance",
        Description: "At the end of your turn, Upgrade {Amount} random card(s) in your discard pile for the rest of this combat.",
        SmartDescription: "At the end of your turn, Upgrade random card(s) in your discard pile for the rest of this combat.");

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return Task.CompletedTask;
        var discard = PileType.Discard.GetPile(Owner.Player!);
        var candidates = discard.Cards.Where(c => c.IsUpgradable).ToList();
        if (candidates.Count == 0) return Task.CompletedTask;
        candidates.StableShuffle(Owner.Player!.RunState.Rng.Shuffle);
        int count = (int)System.Math.Min(Amount, candidates.Count);
        if (count > 0) Flash();
        for (int i = 0; i < count; i++)
        {
            CardCmd.Upgrade(candidates[i]);
        }
        return Task.CompletedTask;
    }
}
