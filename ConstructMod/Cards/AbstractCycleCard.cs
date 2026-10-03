using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

/// <summary>
/// A card that Cycles: when drawn, if its cycle condition is met, it discards itself
/// and draws a replacement. Once per card per turn by default.
/// </summary>
public abstract class AbstractCycleCard : AbstractConstructCard
{
    private bool _cycledThisTurn;

    protected AbstractCycleCard(int baseCost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true, bool autoAdd = true)
        : base(baseCost, type, rarity, target, showInCardLibrary, autoAdd)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return ConstructKeywords.Cycle;
        }
    }

    // Every cycle card gets the Cycle hover tip automatically. Subclasses that add their own tips should
    // still chain through base.ExtraHoverTips (the standard pattern) so this tip is preserved.
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
        }
    }

    public bool CycledThisTurn => _cycledThisTurn;

    public virtual bool CanCycle()
    {
        if (_cycledThisTurn) return false;
        if (Owner == null) return false;
        if (Owner.Creature.HasPower<NoCyclePower>()) return false;
        return true;
    }

    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        if (CanCycle())
        {
            _cycledThisTurn = true;
            await OnCycle(choiceContext);
            CycleCount.Increment(Owner);
            await CycleEvents.NotifyCycle(choiceContext, this);
            await CardCmd.DiscardAndDraw(choiceContext, [this], 1);
        }
        await base.AfterCardDrawnEarly(choiceContext, card, fromHandDraw);
    }

    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        // Reset the per-card cycle flag (once per card per turn). Do NOT reset CycleCount here —
        // AfterPlayerTurnStartEarly fires during/after the draw, so resetting the counter here
        // would wipe cycles that happened during the draw (e.g. a Strike cycling on draw before
        // Rollout is played). CycleCount is reset on AfterSideTurnEnd instead (end of the player's
        // turn), so it's 0 at the start of the next turn's draw.
        _cycledThisTurn = false;
        return base.AfterPlayerTurnStartEarly(choiceContext, player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side, System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants)
    {
        if (side == MegaCrit.Sts2.Core.Combat.CombatSide.Player && Owner != null)
        {
            CycleCount.ResetTurn(Owner);
        }
        await base.AfterSideTurnEnd(choiceContext, side, participants);
    }

    protected virtual Task OnCycle(PlayerChoiceContext choiceContext)
    {
        return Task.CompletedTask;
    }
}
