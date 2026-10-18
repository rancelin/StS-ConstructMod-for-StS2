using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HyperBeam : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Dazed>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(50m, ValueProp.Move),
        new DynamicVar("Dazed", 2m)
    ];

    public HyperBeam() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Hyper Beam",
        "#Deal !Damage! damage.\nPut !Dazed! *Dazed* on top of your draw pile.");

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            if (!IsUpgraded) yield return CardKeyword.Ethereal;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        if (CombatState is not { } combat) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_giant_horizontal_slash").Execute(choiceContext);
        var added = new List<CardPileAddResult>();
        for (var i = 0; i < (int)DynamicVars["Dazed"].BaseValue; i++)
        {
            var dazed = combat.CreateCard(ModelDb.Card<Dazed>(), Owner);
            added.Add(await CardPileCmd.AddGeneratedCardToCombat(dazed, PileType.Draw, Owner, CardPilePosition.Top));
        }
        CardCmd.PreviewCardPileAdd(added); // keep the draw-pile badge in sync (vanilla pattern)
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: drop Ethereal. Mega: +35 damage / +1 Dazed (StS1
        // M_UPGRADE_PLUS_ATTACK_DMG / M_UPGRADE_PLUS_DAZED_AMT).
        if (CurrentUpgradeLevel == 1)
        {
            RemoveKeyword(CardKeyword.Ethereal);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(35m);
            DynamicVars["Dazed"].UpgradeValueBy(1m);
        }
    }
}
