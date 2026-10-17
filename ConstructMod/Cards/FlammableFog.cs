using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Hooks;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class FlammableFog : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new OverheatVar()];

    public FlammableFog() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        Overheat = 5;
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Flammable Fog",
        "#Gain !Block! *Block*. Gain !Block! *Block* next turn.{IfMega:mega: Gain !Block! *Block* in 2 turns.|}\n*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // Block next turn (vanilla BlockNextTurnPower fires on AfterBlockCleared = start of next turn).
        await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars.Block.IntValue, Owner.Creature, this);
        // Mega tier: also block 2 turns from now (FutureTurnBlockPower).
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<FutureTurnBlockPower>(choiceContext, Owner.Creature,
                DynamicVars.Block.IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Regular: +2 Block (5 → 7). Mega: no stat change — the fog pays Block a third time,
        // two turns out (see OnPlay).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Block.UpgradeValueBy(2m);
        }
    }
}
