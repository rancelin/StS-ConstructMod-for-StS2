using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class FocusedBeam : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move)];

    public FocusedBeam() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Focused Beam",
        "#Deal !Damage! damage.{IfMega:mega:\nDraw a card, then put this on top of your draw pile.|\nPut this on top of your draw pile.}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        if (IsMegaUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, 1m, Owner);
        }
        await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +3 damage (9 → 12). Mega: +5 more (12 → 17) and it draws a card before
        // going back on top (StS1 M_UPGRADE_PLUS_ATTACK_DMG + DrawCardAction).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(5m);
        }
    }
}
