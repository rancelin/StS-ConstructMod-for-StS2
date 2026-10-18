using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class FlashFreeze : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(10m, ValueProp.Move), new DynamicVar("FreezeTurns", 2m)];

    public FlashFreeze() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        // No overheat (default -1). Exhausts. Original passes UPGRADE_DESCRIPTION as the base
        // description; we use a normal description and let {IfUpgraded} swap the Innate line.
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Flash Freeze",
        "#Gain !Block! *Block*. Your cards cannot *Overheat* for !FreezeTurns! turn(s).");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<FlashFreezePower>(choiceContext, Owner.Creature,
            DynamicVars["FreezeTurns"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +1 freeze turn (2 → 3) and Innate. Mega: +2 more freeze turns (3 → 5)
        // and Retain (StS1's EXTENDED_DESCRIPTION[0] leads with "Retain.").
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["FreezeTurns"].UpgradeValueBy(1m);
            AddKeyword(CardKeyword.Innate);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["FreezeTurns"].UpgradeValueBy(2m);
            AddKeyword(CardKeyword.Retain);
        }
    }
}
