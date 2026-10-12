using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class SaveState : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("RetainTurns", 1m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    public SaveState() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Save State",
        "#*Retain* your hand this turn.{IfUpgraded:show:\\nDraw 1 card.|}");

    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars["RetainTurns"].BaseValue;
        for (var i = 0; i < (int)amount; i++)
        {
            await PowerCmd.Apply<RetainHandPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        if (IsUpgraded)
        {
            await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, Owner, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
