using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

/// <summary>
/// Switch your Strength and Dexterity, then draw a card.
/// </summary>
public class ModeShift : AbstractConstructCard
{
    private const int DrawAmount = 1;

    public ModeShift() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Mode Shift",
        "Swap your *Strength* and *Dexterity*. Draw a card.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var strength = creature.GetPower<StrengthPower>()?.Amount ?? 0;
        var dexterity = creature.GetPower<DexterityPower>()?.Amount ?? 0;
        if (dexterity - strength != 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, creature, dexterity - strength, creature, this);
        if (strength - dexterity != 0)
            await PowerCmd.Apply<DexterityPower>(choiceContext, creature, strength - dexterity, creature, this);
        await CardPileCmd.Draw(choiceContext, DrawAmount, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
