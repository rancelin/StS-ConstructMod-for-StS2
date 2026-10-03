using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Dampening : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(ConstructKeywords.Cycle)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m, ValueProp.Move),
        new DynamicVar("Draw", 1m)
    ];

    public Dampening() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Dampening",
        "#Your cards cannot Cycle until the end of your next turn.\nGain !Block! *Block*.\nDraw {Draw} card(s).");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Amount = 2 so the power survives this turn's end (tick down to 1) and expires at the end
        // of the NEXT player turn — matching "cannot Cycle until the end of your next turn".
        // (Amount = 1 would remove at the end of THIS turn, letting cards cycle next turn.)
        await PowerCmd.Apply<NoCyclePower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext,
            DynamicVars["Draw"].BaseValue, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1m);
    }
}
