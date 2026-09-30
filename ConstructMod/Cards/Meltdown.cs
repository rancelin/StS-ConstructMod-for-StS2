using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Meltdown : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<MeltdownPower>(12m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Burn>()];

    public Meltdown() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Meltdown",
        "#At the start of your turn, deal {MeltdownPower} damage to ALL enemies and add a *Burn* to your hand.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MeltdownPower>(choiceContext, Owner.Creature,
            DynamicVars["MeltdownPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MeltdownPower"].UpgradeValueBy(2m);
    }
}
