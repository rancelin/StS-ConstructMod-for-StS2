using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Potions;

/// <summary>
/// Swap your Strength and Dexterity, then draw 1 card.
/// (StS1: draw potency cards, potency 1 — StS2 has no potion potency, so the draw is fixed at 1.)
/// </summary>
[Pool(typeof(ConstructPotionPool))]
public class ShiftPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[]
        {
            HoverTipFactory.FromPower<StrengthPower>(null),
            HoverTipFactory.FromPower<DexterityPower>(null)
        };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var creature = Owner.Creature;
        var strength = creature.GetPower<StrengthPower>()?.Amount ?? 0;
        var dexterity = creature.GetPower<DexterityPower>()?.Amount ?? 0;
        if (dexterity - strength != 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, creature, dexterity - strength, creature, null);
        }
        if (strength - dexterity != 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, creature, strength - dexterity, creature, null);
        }
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    public override List<(string, string)>? Localization => new PotionLoc(
        Title: "Shift Potion",
        Description: "#Swap your *Strength* and *Dexterity*. Draw {Cards} card(s).");
}
