using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HardReboot : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DrawCards", 3m)
    ];

    public HardReboot() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Hard Reboot",
        "#Exhaust your hand. Draw {DrawCards} cards.");

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
        var hand = PileType.Hand.GetPile(Owner).Cards.Where(c => c != this).ToList();
        foreach (var card in hand)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext,
            (int)DynamicVars["DrawCards"].BaseValue, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawCards"].UpgradeValueBy(2m);
    }
}
