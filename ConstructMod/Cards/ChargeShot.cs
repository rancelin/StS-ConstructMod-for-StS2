using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ChargeShot : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    private decimal _chargeAccumulated;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar("Charge", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Retain;
        }
    }

    public ChargeShot() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Charge Shot",
        "#Deal !Damage! damage, plus !Charge! for each turn this was Retained.");

    private decimal TrueBaseDamage()
    {
        decimal dmg = 5m;
        if (IsUpgraded) dmg += 2m;
        return dmg;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        ResetCharge();
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return Task.CompletedTask;
        if (PileType.Hand.GetPile(Owner).Cards.Contains(this))
        {
            _chargeAccumulated += DynamicVars["Charge"].BaseValue;
            DynamicVars.Damage.BaseValue = TrueBaseDamage() + _chargeAccumulated;
        }
        else
        {
            ResetCharge();
        }
        return Task.CompletedTask;
    }

    private void ResetCharge()
    {
        _chargeAccumulated = 0m;
        DynamicVars.Damage.BaseValue = TrueBaseDamage();
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +2 damage / +2 charge. Mega: +3 more of each
        // (StS1 M_UPGRADE_PLUS_ATTACK_DMG / M_UPGRADE_PLUS_CHARGE_DMG).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
            DynamicVars["Charge"].UpgradeValueBy(2m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
            DynamicVars["Charge"].UpgradeValueBy(3m);
        }
    }
}
