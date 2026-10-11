using System.Linq;
using MegaCrit.Sts2.Core.Extensions;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

/// <summary>
/// The Enhance mega tier (StS1 EnhanceMegaPower): at the end of the player's turn,
/// force-mega-upgrade N random upgradable cards in the discard pile — regular upgrades
/// for other colors, full mega saturation for Construct cards (ForceUpgradeToMax).
/// </summary>
public class EnhanceMegaPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Enhance (Mega)",
        Description: "#At the end of your turn, #yMega-upgrade {Amount} random card(s) in your discard pile for the rest of this combat.",
        SmartDescription: "#At the end of your turn, #yMega-upgrade {Amount} random card(s) in your discard pile for the rest of this combat.");

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return Task.CompletedTask;
        var discard = PileType.Discard.GetPile(Owner.Player!);
        var candidates = discard.Cards.Where(c =>
            c.IsUpgradable || (c is AbstractConstructCard construct
                && construct.CurrentUpgradeLevel < construct.IntrinsicMaxUpgradeLevel)).ToList();
        if (candidates.Count == 0) return Task.CompletedTask;
        candidates.StableShuffle(Owner.Player!.RunState.Rng.Shuffle);
        int count = (int)System.Math.Min(Amount, candidates.Count);
        if (count > 0) Flash();
        for (int i = 0; i < count; i++)
        {
            AbstractConstructCard.ForceUpgradeToMax(candidates[i]);
        }
        return Task.CompletedTask;
    }
}
