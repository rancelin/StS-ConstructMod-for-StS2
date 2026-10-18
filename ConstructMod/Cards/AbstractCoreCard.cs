using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

/// <summary>
/// Base class for Core cards: 0-cost Cycle cards with an on-cycle effect.
/// When used or cycled while upgraded, a copy is put into the discard pile — an upgraded
/// copy when this Core is mega-upgraded, matching StS1's CloneCore().
/// </summary>
public abstract class AbstractCoreCard : AbstractCycleCard
{
    // 2-tier ladder (SStS1: every core has a mega whose only change is the clone note):
    // +1 = the "put a copy into your discard pile" line appears, +2 = mega (clones are upgraded).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected AbstractCoreCard(CardType type, TargetType target, CardRarity rarity = CardRarity.Uncommon)
        : base(0, type, rarity, target, showInCardLibrary: false)
    {
        // NOTE: autoAdd stays true (cores belong to the Construct pool). Setting
        // autoAdd:false leaves the card pool-less and CardModel.get_Pool() throws
        // when the engine renders it (turn-loop death on draw). This also matches
        // StS1, where cores are registered via addCard() and can appear as rewards.
        // Rarity is per-core (only Battery Core is Rare in StS1); weight comes from it.
    }

    public override List<(string, string)>? Localization =>
        new CardLoc(CoreName,
            $"#*Cycle*. When cycled: {CoreCycleText}\n{{IfPlusOne:plusone:Put a {CoreName}{{CorePlus}} into your discard pile.|}}");

    protected abstract string CoreName { get; }
    protected abstract string CoreCycleText { get; }

    /// <summary>
    /// "{CorePlus}" resolves to "+" while mega-upgraded (StS1's mega clone line reads
    /// "Put a X Core+ into your discard pile"), otherwise "". Injected as a plain string arg
    /// rather than nesting an {IfMega:...} formatter inside the tier branch.
    /// </summary>
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("CorePlus", IsMegaUpgraded ? "+" : "");
    }

    /// <summary>
    /// The on-cycle effect. Runs before the replacement draw.
    /// </summary>
    protected abstract Task OnCoreCycle(PlayerChoiceContext choiceContext);
    protected abstract CardModel CanonicalCore { get; }

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
        if (!IsUpgraded || CombatState is not { } combat) return;
        var copy = combat.CreateCard(CanonicalCore, Owner);
        // StS1 CloneCore(): a mega-upgraded Core clones an UPGRADED copy ("Core+");
        // a merely-upgraded Core clones a base copy. (QuantumEgg's forced upgrade
        // has no port equivalent — the relic was not carried over.)
        if (IsMegaUpgraded && copy.IsUpgradable) CardCmd.Upgrade(copy);
        var added = await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Discard, Owner, CardPilePosition.Top);
        // Vanilla pattern for generating into a hidden pile (Turbo, Metamorphosis, ...): the discard
        // button's count badge is advanced by the fly-to-pile animation, not by the pile contents.
        CardCmd.PreviewCardPileAdd(added);
    }
}
