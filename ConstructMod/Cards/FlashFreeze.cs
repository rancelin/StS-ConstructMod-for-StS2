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

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override List<(string, string)>? Localization => new CardLoc("Flash Freeze",
        "#Gain !Block! *Block*. Your cards cannot *Overheat* for {FreezeTurns} turn(s). *Exhaust*.{IfUpgraded:show: *Innate*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<FlashFreezePower>(choiceContext, Owner.Creature,
            DynamicVars["FreezeTurns"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Original: +1 freeze turn + Innate on upgrade, then mega = +2 freeze turns.
        // Folded: +3 freeze turns + Innate.
        DynamicVars["FreezeTurns"].UpgradeValueBy(3m);
        AddKeyword(CardKeyword.Innate);
    }
}
