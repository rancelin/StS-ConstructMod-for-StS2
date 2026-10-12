using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Overclock : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Cards", 2m)];

    public Overclock() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Overclock",
        "#At the start of your turn, draw {Cards} cards and add a *Burn* to your hand.");

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromCard<Burn>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<OverclockPower>(choiceContext, Owner.Creature,
            DynamicVars["Cards"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
