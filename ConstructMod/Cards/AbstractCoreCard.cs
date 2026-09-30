using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Cards;

/// <summary>
/// Base class for Core cards: 0-cost Cycle cards with an on-cycle effect.
/// When used or cycled while upgraded, a copy is put into the discard pile.
/// </summary>
public abstract class AbstractCoreCard : AbstractCycleCard
{
    protected AbstractCoreCard(CardType type, TargetType target)
        : base(0, type, CardRarity.Uncommon, target, showInCardLibrary: false)
    {
    }

    public override List<(string, string)>? Localization =>
        new CardLoc(CoreName,
            $"#*Cycle*. When cycled: {CoreCycleText}\n{{IfUpgraded:show:Put a {CoreName} into your discard pile.|}}");

    protected abstract string CoreName { get; }
    protected abstract string CoreCycleText { get; }

    /// <summary>
    /// The on-cycle effect. Runs before the replacement draw.
    /// </summary>
    protected abstract Task OnCoreCycle(PlayerChoiceContext choiceContext);

    protected override async Task OnCycle(PlayerChoiceContext choiceContext)
    {
        await OnCoreCycle(choiceContext);
        await CloneCoreIfNeeded();
    }

    protected async Task PlayCoreEffect(PlayerChoiceContext choiceContext)
    {
        await OnCoreCycle(choiceContext);
        await CloneCoreIfNeeded();
    }

    private async Task CloneCoreIfNeeded()
    {
        if (!IsUpgraded) return;
        var clone = CreateClone();
        await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Discard, Owner, CardPilePosition.Top);
    }
}
