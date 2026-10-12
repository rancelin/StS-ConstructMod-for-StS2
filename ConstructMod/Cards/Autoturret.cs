using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Autoturret : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<AutoturretPower>("Turret", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var tip in base.ExtraHoverTips) yield return tip;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
        }
    }

    public Autoturret() : base(1, CardType.Power, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Autoturret",
        "#Whenever a card *Cycles*, deal {Turret} damage to a random enemy.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AutoturretPower>(choiceContext, Owner.Creature,
            DynamicVars["Turret"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Turret"].UpgradeValueBy(1m);
    }
}
