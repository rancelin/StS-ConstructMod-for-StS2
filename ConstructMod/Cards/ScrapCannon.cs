using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ScrapCannon : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        new IntVar("Exhaust", 1)
    ];

    public ScrapCannon() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Scrap Cannon",
        "#Deal !Damage! damage.\n{IfMega:mega:Exhaust up to !Exhaust! cards|Exhaust !Exhaust! card}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }
        int count = DynamicVars["Exhaust"].IntValue;
        // StS1: the base tier forces you to exhaust exactly 1 card (ExhaustAction's default
        // anyNumber/canPickZero are both false), while the mega tier's "Exhaust up to 2 cards"
        // passes canPickZero — so the player may confirm with fewer, including none.
        int min = IsMegaUpgraded ? 0 : count;
        var cards = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, min, count),
            context: choiceContext, player: Owner, filter: null, source: this)).ToList();
        foreach (var card in cards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +3 damage (9 → 12). Mega: +5 more damage and one more card exhausted
        // (StS1 M_UPGRADE_PLUS_ATTACK_DMG / M_UPGRADE_PLUS_NUM_EXHAUST).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(5m);
            DynamicVars["Exhaust"].UpgradeValueBy(1m);
        }
    }
}
