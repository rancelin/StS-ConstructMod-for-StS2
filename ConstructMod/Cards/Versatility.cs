using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Versatility : AbstractConstructCard
{
    // 2-tier ladder: +1 = +3/+3, +2 = mega (+1/+1, draw 2 on play).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new BlockVar(7m, ValueProp.Move)
    ];

    public Versatility() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Versatility",
        "#If the target intends to attack, gain !Block! *Block*. Otherwise, deal !Damage! damage.{IfMega:mega:\nDraw 2 cards.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target?.Monster is { IntendsToAttack: true })
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
        else if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }
        if (IsMegaUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, 2m, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: +3/+3 (7 → 10).
            DynamicVars.Damage.UpgradeValueBy(3m);
            DynamicVars.Block.UpgradeValueBy(3m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            // Mega: +1/+1 (10 → 11); the Draw 2 moves here (OnPlay, gated on IsMegaUpgraded).
            DynamicVars.Damage.UpgradeValueBy(1m);
            DynamicVars.Block.UpgradeValueBy(1m);
        }
    }
}
