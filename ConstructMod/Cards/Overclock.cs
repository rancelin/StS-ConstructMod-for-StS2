using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
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
        "{IfUpgraded:show:Draw 4 cards. NL |}At the start of your turn, draw {Cards} cards and add a *Burn* to your hand.");

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
        if (IsUpgraded) await CardPileCmd.Draw(choiceContext, 4m, Owner);
        await PowerCmd.Apply<OverclockPower>(choiceContext, Owner.Creature,
            DynamicVars["Cards"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

public class OverclockPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Overclock",
        Description: "At the start of your turn, draw {Amount} cards and add {Amount} *Burn* to your hand.",
        SmartDescription: "At the start of your turn, draw {Amount} cards and add {Amount} *Burn* to your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState is not { } combat) return;
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
        for (var i = 0; i < Amount; i++)
        {
            var burn = combat.CreateCard(ModelDb.Card<Burn>(), Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, Owner.Player);
        }
    }
}
