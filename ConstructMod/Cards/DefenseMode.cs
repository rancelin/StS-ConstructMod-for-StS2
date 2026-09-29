using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class DefenseMode : AbstractCycleCard
{
    public DefenseMode() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        AddKeyword(CardKeyword.Retain);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[]
        {
            HoverTipFactory.FromPower<DexterityPower>(null),
            HoverTipFactory.FromPower<StrengthPower>(null)
        };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DexterityPower>("Dexterity", 2m),
        new PowerVar<StrengthPower>("Strength", -2m)
    };

    public override List<(string, string)>? Localization => new CardLoc("Defense Mode",
        "Retain. Gain {Dexterity} Dexterity. Lose {Strength} Strength.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<DexterityPower>(choiceContext, creature, DynamicVars["Dexterity"].BaseValue, creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, creature, DynamicVars["Strength"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Dexterity"].UpgradeValueBy(1m);
        DynamicVars["Strength"].UpgradeValueBy(-1m);
    }
}
