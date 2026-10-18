using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Reserves : AbstractCycleCard
{
    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(3),
        new CardsVar(3),
        new DynamicVar("HpThreshold", 20m)
    ];

    // 2-tier ladder: +1 = +10 threshold, +2 = mega (+20 threshold).
    public override int IntrinsicMaxUpgradeLevel => 2;

    public Reserves() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Reserves",
        "#*Cycle* if your HP is above !HpThreshold!.\nGain {Energy:energyIcons()}. Draw {Cards} cards.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        return Owner.Creature.CurrentHp > DynamicVars["HpThreshold"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext,
            (int)DynamicVars["Cards"].BaseValue, Owner, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: +10 threshold (20 → 30). Mega: +20 threshold (30 → 50).
        if (CurrentUpgradeLevel == 1) DynamicVars["HpThreshold"].UpgradeValueBy(10m);
        else if (CurrentUpgradeLevel == 2) DynamicVars["HpThreshold"].UpgradeValueBy(20m);
    }
}
