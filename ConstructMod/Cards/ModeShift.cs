using System.Threading.Tasks;
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

/// <summary>
/// Switch your Strength and Dexterity, then draw a card.
/// </summary>
public class ModeShift : AbstractConstructCard
{
    // COMMON in StS1 (not Basic): reward-eligible. A prior testing commit set Basic to
    // hide it from rewards, but faithful-to-original takes precedence — it belongs in
    // the pool. (Basic never appears in rewards, which is also why the Basic Heated
    // challenge cards need no pool action: they're already reward-excluded.)

    // 2-tier ladder: +1 = cost 1 → 0, +2 = mega (+1 draw, Retain).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar("Cards", 1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            // Mega tier only.
            if (IsMegaUpgraded) yield return CardKeyword.Retain;
        }
    }
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>(null),
            HoverTipFactory.FromPower<DexterityPower>(null)];

    public ModeShift() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Mode Shift",
        "#Swap your *Strength* and *Dexterity*. Draw {Cards} card(s).");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var strength = creature.GetPower<StrengthPower>()?.Amount ?? 0;
        var dexterity = creature.GetPower<DexterityPower>()?.Amount ?? 0;
        if (dexterity - strength != 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, creature, dexterity - strength, creature, this);
        if (strength - dexterity != 0)
            await PowerCmd.Apply<DexterityPower>(choiceContext, creature, strength - dexterity, creature, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: cost 1 → 0.
            EnergyCost.UpgradeBy(-1);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            // Mega: +1 draw (1 → 2), Retain.
            DynamicVars["Cards"].UpgradeValueBy(1m);
            AddKeyword(CardKeyword.Retain);
        }
    }
}
