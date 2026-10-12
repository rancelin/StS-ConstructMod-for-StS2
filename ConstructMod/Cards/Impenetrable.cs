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

namespace ConstructMod.Cards;

public class Impenetrable : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        EnergyHoverTip,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(25m, ValueProp.Move),
        new EnergyVar(1)
    ];

    public Impenetrable() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Impenetrable",
        "#Gain !Block! *Block*. Gain {Energy:energyIcons()} next turn.");

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars.Energy.IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
