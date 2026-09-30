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
        new DynamicVar("Cards", 3m),
        new DynamicVar("HpThreshold", 10m)
    ];

    public Reserves() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Reserves",
        "#*Cycle* if your HP is above {HpThreshold}.\nGain [E] [E] [E]. Draw {Cards} cards. *Exhaust*.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        return Owner.Creature.CurrentHp > DynamicVars["HpThreshold"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(3m, Owner);
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext,
            (int)DynamicVars["Cards"].BaseValue, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HpThreshold"].UpgradeValueBy(5m);
    }
}
